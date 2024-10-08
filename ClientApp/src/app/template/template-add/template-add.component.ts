import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { CodezenUiRendererComponent, Field, RendererType } from 'codezen-ui-layout';
import { Observable, Subject, takeUntil } from 'rxjs';
import { EntityDataService } from 'src/app/angular-app-services/entity-data.service';
import { LayoutService } from 'src/app/angular-app-services/layout.service';
import { SweetAlertService } from 'src/app/angular-app-services/sweet-alert.service';
import { TokenService } from 'src/app/angular-app-services/token.service';
import { _camelToSentenceCase, _toSentenceCase } from 'src/app/library/utils';
import { PatchField } from './patch-field.class';
import { compare } from 'fast-json-patch';

@Component({
  selector: 'app-template-add',
  templateUrl: './template-add.component.html',
  styleUrl: './template-add.component.scss'
})
export class TemplateAddComponent implements OnInit, OnDestroy {
  @Input() entityName: string = '';
  @Input() id: string = '';
  @Output() saved = new EventEmitter<boolean>();

  public entityDisplayName: string = '';
  public layoutData!: any[];
  public layoutEmptyData!: any[];
  public rendererType = this.id ? RendererType.EDIT : RendererType.ADD;
  public selectorPrefix = 'use-cz-';
  private record: any = null;

  @ViewChild('layoutRenderer') layoutRenderer!: CodezenUiRendererComponent;

  private destroy = new Subject();

  constructor(
    private dialogRef: MatDialogRef<TemplateAddComponent>,
    private entityDataService: EntityDataService,
    private layoutService: LayoutService,
    private sweetAlertService: SweetAlertService,
    private tokenService: TokenService
  ) {
  }

  ngOnInit(): void {
    this.entityDisplayName = _camelToSentenceCase(this.entityName);
    this.rendererType = this.id ? RendererType.EDIT : RendererType.ADD;
    this.getLayout(this.entityName, this.id ? 'Edit' : 'Add');
  }

  ngOnDestroy(): void {
    this.destroy.next(true);
    this.destroy.complete();
  }

  closeDialog(status: boolean = false): void {
    this.dialogRef.close(status);
  }

  onSubmit(): void {
    const isValid = this.layoutRenderer.validate();
    if (!isValid) return;
    const tenantId = this.tokenService.getTenantId(),
      data = this.unflattenObject(this.layoutRenderer.value, tenantId ?? '');
    let apiCall: Observable<any>;
    if (this.id) {
      data.Id = this.id;
      const record = { ...this.record },
        patch = (compare(record, data) as PatchField[]).filter(o => !o.path.includes('.'));
      apiCall = this.entityDataService.patchRecordById(this.entityName, this.id, patch);
    }
    else {
      apiCall = this.entityDataService.addRecord(this.entityName, data);
    }
    apiCall?.pipe(takeUntil(this.destroy))
      .subscribe({
        next: data => {
          if (data) {
            this.sweetAlertService.showSuccess(`${_toSentenceCase(_camelToSentenceCase(this.entityName))} has been ${this.id ? 'updated' : 'added'}.`);
            this.saved.emit(true);
          }
        }
      });
  }

  private getAllDropDownFields(fields: any[]): any[] {
    let dropDownFields: any[] = [];

    fields?.forEach(field => {
      if (field.entityName) {
        dropDownFields.push(field);
      }
      if (field.fields && Array.isArray(field.fields)) {
        dropDownFields = dropDownFields.concat(this.getAllDropDownFields(field.fields));
      }
    });

    return dropDownFields;
  }

  private getLayout(entityName: string, fileName: string): void {
    this.layoutService.getLayout(entityName, fileName)
      ?.pipe(takeUntil(this.destroy))
      .subscribe({
        next: data => {
          const lookupView = this.findNodesWithLookupView(data);
          this.getLookupViewLayout(lookupView, data);
          if (this.id) {
            this.getRecord(this.id, data);
          } else {
            data = this.entityDataService.mapFieldValue(data, this.record);
            this.loadDropDownData(data);
          }
        }
      });
  }

  private getRecord(id: string, layoutData: any[]): void {
    const fields = this.entityDataService.getFields(layoutData);
    this.entityDataService.getRecordById(this.entityName, id, fields)
      ?.pipe(takeUntil(this.destroy))
      .subscribe({
        next: data => {
          this.record = this.flattenObject(data);
          layoutData = this.entityDataService.mapFieldValue(layoutData, this.record);
          this.loadDropDownData(layoutData);
        }
      });
  }

  private loadDropDownData(data: any[]): void {
    const dropDownFields = this.getAllDropDownFields(data);

    if (dropDownFields.length === 0) {
      this.layoutData = data;
      return;
    }

    dropDownFields.forEach(field => {
      this.entityDataService.getRecords(field.entityName)
        .pipe(takeUntil(this.destroy))
        .subscribe({
          next: (data) => {
            if (Array.isArray(data)) {
              field.dataSource = data;
              field.textField = 'name';
              field.valueField = 'id';
              field.isValuePrimitive = true;
            }
          }
        });
    });

    this.layoutData = data;
  }

  private findNodesWithLookupView(data: Field[]): Field[] {
    let result: Field[] = [];

    data.forEach(node => {
      if (node.lookupView) {
        result.push(node);
      }

      if (node.fields && Array.isArray(node.fields)) {
        result = result.concat(this.findNodesWithLookupView(node.fields));
      }
    });

    return result;
  }

  private getLookupViewLayout(lookupView: Field[], layoutData: Field[]): void {
    lookupView?.forEach((item, index, self) => {
      const firstOccurance = self.findIndex(o => o.entityName === item.entityName && o.lookupView === item.lookupView);
      if (firstOccurance >= index) {
        this.layoutService.getLayout(item.entityName ?? '', item.lookupView)
          .pipe(takeUntil(this.destroy))
          .subscribe({
            next: (response) => {
              this.updateLayout(item.entityName ?? '', item.lookupView, response, this.layoutData ?? layoutData);
            }
          });
      }
    });
  }

  private updateLayout(entityName: string, lookupView: string, response: any, layout: Field[]): void {
    layout.forEach(field => {
      if (field.entityName === entityName && field.lookupView === lookupView) {
        field.lookupViewTemplate = response;
      }

      if (field.fields && field.fields.length > 0) {
        this.updateLayout(entityName, lookupView, response, field.fields);
      }
    });
  }

  private flattenObject(record: any): any {
    const result: any = { ...record };

    for (const key in record) {
      if (Object.prototype.hasOwnProperty.call(record, key) && typeof record[key] === 'object' && record[key] !== null) {
        const nestedObj = record[key];

        for (const nestedKey in nestedObj) {
          if (Object.prototype.hasOwnProperty.call(nestedObj, nestedKey)) {
            const newKey = `${key}.${nestedKey}`;
            result[newKey] = nestedObj[nestedKey];
          }
        }
      }
    }

    return result;
  }

  private unflattenObject(obj: any, tenantId: string): any {
    const result: any = {};

    for (const key in obj) {
      if (Object.prototype.hasOwnProperty.call(obj, key)) {
        const keys = key.split('.');
        if (keys.length > 1 && !result[keys[0]]) {
          const existingIdKey = keys[0].split('_')[0],
            recordId = this.id ? this.record[existingIdKey] : crypto.randomUUID();
          result[keys[0]] = {
            'TenantId': tenantId,
            'Id': recordId
          };
        }
        keys.reduce((acc, k, index) => {
          if (index === keys.length - 1) {
            acc[k] = obj[key];
          } else {
            if (!acc[k]) {
              acc[k] = {};
            }
          }
          return acc[k];
        }, result);
      }
    }

    return result;
  }
}
