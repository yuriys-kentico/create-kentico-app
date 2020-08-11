import { IUserWidgetProperty } from './IUserWidgetProperty';

export interface IEditableProperty {
  property: Partial<IUserWidgetProperty>;
  defaultMode: 'edit' | 'view';
}
