import clsx from 'clsx';
import React, { FC, useState } from 'react';
import { DndProvider } from 'react-dnd';
import { HTML5Backend } from 'react-dnd-html5-backend';
import { useTranslation } from 'react-i18next';

import { createStyles, makeStyles } from '@material-ui/styles';

import { IEditableProperty } from './IEditableProperty';
import { IUserWidgetProperty } from './IUserWidgetProperty';
import { PropertyRow } from './PropertyRow';

interface IPropertiesEditorProps {
  value: IUserWidgetProperty[];
  name: string;
  formComponents: { [key: string]: string };
}

const useStyles = makeStyles(() =>
  createStyles({
    root: { maxWidth: 600 },
    validation: {
      padding: '0 0 0 6px !important',
      margin: '0 0 0 8px !important',
    },
  })
);

export const PropertiesEditor: FC<IPropertiesEditorProps> = ({ value, name, formComponents }) => {
  const styles = useStyles();

  const [localize] = useTranslation();

  const [properties, setProperties] = useState<IEditableProperty[]>(
    value.map((property) => ({ property, defaultMode: 'view' }))
  );
  const [validation, setValidation] = useState('');

  const isValid = (newProperty: Partial<IUserWidgetProperty>, newIndex: number) => {
    let validation = [];

    if (!newProperty.name) {
      validation.push('Properties must have names.');
    }

    if (
      properties
        .filter((_, index) => index !== newIndex)
        .some((property) => property.property.name === newProperty.name)
    ) {
      validation.push('Properties must have unique names.');
    }

    if (!newProperty.name?.match(/^@?[\p{L}\p{Nl}_][\p{Cf}\p{L}\p{Mc}\p{Mn}\p{Nd}\p{Nl}\p{Pc}]*$/u)) {
      validation.push('Property names must be valid identifiers.');
    }

    if (!newProperty.typeName) {
      validation.push('Properties must have a type.');
    }

    if (!newProperty.formComponentIdentifier) {
      validation.push('Properties must have a form component.');
    }

    if (validation.length) {
      setValidation(validation.join(' '));

      return false;
    }

    setValidation('');

    return true;
  };

  const validate = (index: number) => (property: Partial<IUserWidgetProperty> | null, onValid?: () => void) => {
    if (property === null) {
      delete properties[index];
    } else {
      if (!isValid(property, index)) {
        return;
      }

      onValid?.();

      properties[index] = { property, defaultMode: 'view' };
    }

    setProperties(properties.filter((property) => property !== undefined));
  };

  const setOrder = (dragIndex: number, hoverIndex: number) => {
    const dragged = properties[dragIndex];

    properties.splice(dragIndex, 1);
    properties.splice(hoverIndex, 0, dragged);

    setProperties([...properties]);
  };

  const addProperty = () => {
    setProperties([...properties, { property: {}, defaultMode: 'edit' }]);
  };

  return (
    <DndProvider backend={HTML5Backend}>
      <div className={styles.root}>
        <table className='table table-hover'>
          <thead>
            <tr>
              <th scope='col'>
                <div className='unigrid-menu-panel'>{localize('unigrid.actions')}</div>
              </th>
              {[
                localize('general.name'),
                localize('general.type'),
                localize('app.boilerplate.ui.userwidget.properties.formcomponent'),
                localize('app.boilerplate.ui.userwidget.properties.defaultvalue'),
                localize('general.label'),
                localize('app.boilerplate.ui.userwidget.properties.tooltip'),
                localize('app.boilerplate.ui.userwidget.properties.explanation'),
              ].map((column, index) => (
                <th key={index} scope='col'>
                  <div>{column}</div>
                </th>
              ))}
              <th className='filling-column' scope='col' />
            </tr>
          </thead>
          <tbody>
            {properties.map((property, index) => (
              <PropertyRow
                key={property.property.name}
                index={index}
                formComponents={formComponents}
                editableProperty={property}
                validate={validate(index)}
                setOrder={setOrder}
              />
            ))}
          </tbody>
        </table>
        <div className='btn btn-primary' onClick={addProperty}>
          {localize('app.boilerplate.ui.userwidget.properties.addproperty')}
        </div>
        {validation !== '' && (
          <div className={clsx('alert-error', 'alert', styles.validation)}>
            <div className='alert-label'>{validation}</div>
          </div>
        )}
        <input
          hidden
          readOnly
          name={name}
          value={JSON.stringify(properties.map((property, index) => ({ ...property.property, order: index })))}
        />
      </div>
    </DndProvider>
  );
};
