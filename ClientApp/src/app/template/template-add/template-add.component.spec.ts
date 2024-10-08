import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { ReactiveFormsModule } from '@angular/forms';
import { TemplateAddComponent } from './template-add.component';
import { EntityDataService } from 'src/app/angular-app-services/entity-data.service';
import { LayoutService } from 'src/app/angular-app-services/layout.service';
import { SweetAlertService } from 'src/app/angular-app-services/sweet-alert.service';
import { of } from 'rxjs';
import { CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA } from '@angular/core';

describe('TemplateAddComponent', () => {
    let component: TemplateAddComponent;
    let fixture: ComponentFixture<TemplateAddComponent>;
    let entityDataServiceMock: jasmine.SpyObj<EntityDataService>;
    let layoutServiceMock: jasmine.SpyObj<LayoutService>;
    let sweetAlertServiceMock: jasmine.SpyObj<SweetAlertService>;

    beforeEach(
        waitForAsync(() => {
            entityDataServiceMock = jasmine.createSpyObj('EntityDataService', ['addRecord', 'editRecordById', 'getRecordById', 'getFields', 'mapFieldValue']);
            layoutServiceMock = jasmine.createSpyObj('LayoutService', ['getLayout']);
            sweetAlertServiceMock = jasmine.createSpyObj('SweetAlertService', ['showSuccess']);

            TestBed.configureTestingModule({
                declarations: [TemplateAddComponent],
                imports: [ReactiveFormsModule, MatDialogModule],
                providers: [
                    { provide: MatDialogRef, useValue: {} },
                    { provide: MAT_DIALOG_DATA, useValue: {} },
                    { provide: EntityDataService, useValue: entityDataServiceMock },
                    { provide: LayoutService, useValue: layoutServiceMock },
                    { provide: SweetAlertService, useValue: sweetAlertServiceMock }
                ],
                schemas: [
                    NO_ERRORS_SCHEMA,
                    CUSTOM_ELEMENTS_SCHEMA
                ],
            }).compileComponents();

            fixture = TestBed.createComponent(TemplateAddComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
        }));

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should initialize form with default values', () => {
        const layoutData = [
            {
                dataType: 'section', fields: [
                    { fieldName: 'name', dataType: 'string', required: true },
                    { fieldName: 'age', dataType: 'numeric', required: false }
                ]
            }
        ];
        layoutServiceMock.getLayout.and.returnValue(of(layoutData));
        component.ngOnInit();
    });

    it('should patch form with existing data', () => {
        const layoutData = [
            {
                dataType: 'section', fields: [
                    { fieldName: 'name', dataType: 'string', required: true },
                    { fieldName: 'age', dataType: 'numeric', required: false }
                ]
            }
        ];
        const existingData = { name: 'John', age: 30 };
        layoutServiceMock.getLayout.and.returnValue(of(layoutData));
        entityDataServiceMock.getRecordById.and.returnValue(of(existingData));
        component.id = '123';
        component.ngOnInit();
    });

    xit('should add record', () => {
        const layoutData = [
            {
                dataType: 'section', fields: [
                    { fieldName: 'name', dataType: 'string', required: true },
                    { fieldName: 'age', dataType: 'numeric', required: false }
                ]
            }
        ];
        const newData = { name: 'John', age: 30 };
        layoutServiceMock.getLayout.and.returnValue(of(layoutData));
        entityDataServiceMock.addRecord.and.returnValue(of(true));
        component.ngOnInit();
        component.onSubmit();
        expect(entityDataServiceMock.addRecord).toHaveBeenCalledWith('', newData);

        const actualCallArgument = sweetAlertServiceMock.showSuccess.calls.mostRecent().args[0].trim();
        expect(actualCallArgument).toEqual('has been added.');
    });

    xit('should edit record', () => {
        const layoutData = [
            {
                dataType: 'section', fields: [
                    { fieldName: 'name', dataType: 'string', required: true },
                    { fieldName: 'age', dataType: 'numeric', required: false }
                ]
            }
        ];

        layoutServiceMock.getLayout.and.returnValue(of(layoutData));
        entityDataServiceMock.editRecordById.and.returnValue(of(true));

        fixture = TestBed.createComponent(TemplateAddComponent);
        component = fixture.componentInstance;

        component.ngOnInit();
        component.onSubmit();
    });
});
