import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AppConfigService } from '../app-config.service';
import { PatchField } from '../template/template-add/patch-field.class';
import { Field } from 'codezen-ui-layout';

@Injectable({
    providedIn: 'root'
})
export class EntityDataService {
    constructor(private http: HttpClient) { }

    public addRecord(entityName: string, data: any): Observable<any> {
        return this.http.post<any>(`${this.route}/${entityName}`, data);
    }

    public deleteRecordById(entityName: string, id: string): Observable<any> {
        return this.http.delete(`${this.route}/${entityName}/${id}`);
    }

    public editRecordById(entityName: string, id: string, data: any): Observable<any> {
        return this.http.put<any>(`${this.route}/${entityName}/${id}`, data);
    }

    public patchRecordById(entityName: string, id: string, fields: PatchField[]): Observable<any> {
        return this.http.patch<any>(`${this.route}/${entityName}/${id}`, fields);
    }

    public getRecords(entityName: string, filters: any[] = [], searchTerm: string = '', pageNumber: number = 1, pageSize: number = 10, sortField: string = '', sortOrder: string = 'asc'): Observable<any[]> {
        const params: any = {};

        if (filters.length > 0) {
            params.filters = JSON.stringify(filters);
        }
        if (searchTerm) {
            params.searchTerm = searchTerm;
        }
        if (pageNumber > 0) {
            params.pageNumber = pageNumber;
            if (pageSize > 0) {
                params.pageSize = pageSize;
            }
        }
        if (sortField) {
            params.sortField = sortField;
            if (sortOrder) {
                params.sortOrder = sortOrder;
            }
        }
        return this.http.get<any[]>(`${this.route}/${entityName}`, { params });
    }

    public getRecordById(entityName: string, id: string, fields: string[] = []): Observable<any> {
        return this.http.get<any>(`${this.route}/${entityName}/${id}`, { params: { fields: fields.join(',') } });
    }

    public getFields(layout: any[], fields: string[] = []): string[] {
        layout?.forEach(field => {
            if (['section', 'tab', 'groupfield'].indexOf(field.dataType.toLowerCase()) > -1) {
                fields = this.getFields(field.fields, fields);
            } else if (field.fieldName && !fields.includes(field.fieldName)) {
                fields.push(`${field.fieldName}`);
                if (field.dataType?.toLowerCase() === 'guid' && field.entityName) {
                    fields.push(`${field.fieldName}_${field.entityName}.${field.valueField ?? 'id'}`);
                    fields.push(`${field.fieldName}_${field.entityName}.${field.textField ?? 'name'}`);
                }
            }
        });
        return fields;
    }

    public mapFieldValue(layout: Field[], record: any, relatedEntities: { [key: string]: any[]; } = {}, isPreview: boolean = false): Field[] {
        if (layout && (record || relatedEntities)) {
            layout = this.mapFieldValueRecursive(layout, record, isPreview, relatedEntities);
        }
        return layout;
    }

    private mapFieldValueRecursive(layout: Field[], record: any, isPreview: boolean, relatedEntities: { [key: string]: any[]; } = {}): any {
        const dateFormat = 'MM/dd/yyyy',
            dateTimeFormat = `${dateFormat} hh:mm a`;
        return layout?.map(node => {
            if (node.dataType.toLowerCase() === 'guid') {
                node.dataSource = relatedEntities?.[node.entityName ?? ''] ?? node.dataSource;
            }
            else if (node.dataType?.toLowerCase() === 'date') {
                node.format = dateFormat;
            }
            else if (node.dataType?.toLowerCase() === 'datetime') {
                node.format = dateTimeFormat;
            }

            if (node.removeUrl?.startsWith('BASEURL')) {
                node.removeUrl = node.removeUrl.replace('BASEURL', AppConfigService.appConfig ? AppConfigService.appConfig.api.url : '');
            }
            if (node.saveUrl?.startsWith('BASEURL')) {
                node.saveUrl = node.saveUrl.replace('BASEURL', AppConfigService.appConfig ? AppConfigService.appConfig.api.url : '');
            }

            return {
                ...node,
                fields: node.fields ? this.mapFieldValueRecursive(node.fields, record, isPreview, relatedEntities) : [],
                value: node.fieldName ? this.getFormattedData(record, node, isPreview) : ''
            };
        });
    }

    private getFormattedData(record: any, fieldInfo: Field, isPreview: boolean): any {
        if (!fieldInfo?.dataType || !fieldInfo?.fieldName || !record) return '';
        const fieldName = fieldInfo.fieldName,
            data = record[fieldName] ?? null;
        if (fieldInfo.dataType.toLowerCase() === 'guid' && isPreview) {
            const refProp = `${fieldName}_${fieldInfo.fieldName}`.toLowerCase(),
                refPropertyName = record[refProp] || Object.keys(record)?.find(o => o.toLowerCase() === refProp),
                refObject = refPropertyName ? record[refPropertyName] : null;
            return refObject?.[fieldInfo.textField ?? ''] ?? data;
        } else if (fieldInfo.dataType.toLowerCase() === 'date' || fieldInfo.dataType.toLowerCase() === 'datetime') {
            const date = Date.parse(data + 'Z'),
                localDate = isNaN(date) ? date : new Date(data + 'Z');
            return localDate;
        }
        return data;
    }

    private get route(): string {
        const baseUrl = AppConfigService.appConfig ? AppConfigService.appConfig.api.url : '';
        return `${baseUrl}/api`;
    }
}
