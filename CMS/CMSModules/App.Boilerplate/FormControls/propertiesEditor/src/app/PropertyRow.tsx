import clsx from 'clsx';
import React, { FC, useRef, useState } from 'react';
import { useDrag, useDrop } from 'react-dnd';
import { useTranslation } from 'react-i18next';

import { createStyles, makeStyles } from '@material-ui/styles';

import { IEditableProperty } from './IEditableProperty';
import { IUserWidgetProperty } from './IUserWidgetProperty';

interface IPropertyRowProps {
  index: number;
  formComponents: { [key: string]: string };
  editableProperty: IEditableProperty;
  validate: (property: Partial<IUserWidgetProperty> | null, onValid?: () => void) => void;
  setOrder: (dragIndex: number, hoverIndex: number) => void;
}

interface IDraggableProperty {
  index: number;
  type: string;
}

const useStyles = makeStyles(() =>
  createStyles({
    input: { minWidth: 100 },
    select: { minWidth: 100, height: '22px !important', padding: '0px !important' },
  })
);

const propertyType = 'property';

export const PropertyRow: FC<IPropertyRowProps> = ({ index, formComponents, editableProperty, validate, setOrder }) => {
  const { property, defaultMode } = editableProperty;

  const styles = useStyles();

  const [localize] = useTranslation();

  const ref = useRef<HTMLTableRowElement | null>(null);

  const [mode, setMode] = useState(defaultMode);
  const [name, setName] = useState(property.name);
  const [typeName, setTypeName] = useState(property.typeName);
  const [formComponentIdentifier, setFormComponentIdentifier] = useState(property.formComponentIdentifier);
  const [defaultValue, setDefaultValue] = useState(property.defaultValue);
  const [label, setLabel] = useState(property.label);
  const [tooltip, setTooltip] = useState(property.tooltip);
  const [explanationText, setExplanationText] = useState(property.explanationText);

  const edit = () => {
    setMode('edit');
  };

  const deleteProperty = () => {
    validate(null);
  };

  const apply = () => {
    validate(
      {
        name,
        typeName,
        formComponentIdentifier,
        defaultValue,
        label,
        tooltip,
        explanationText,
      },
      () => setMode('view')
    );
  };

  const cancel = () => {
    if (!Object.keys(property).length) {
      validate(null);

      return;
    }

    setMode('view');
    setName(property.name);
    setTypeName(property.typeName);
    setFormComponentIdentifier(property.formComponentIdentifier);
    setDefaultValue(property.defaultValue);
    setLabel(property.label);
    setTooltip(property.tooltip);
    setExplanationText(property.explanationText);
  };

  const setFormComponent = (formComponent: string) => {
    const typeName = formComponents[formComponent];

    setFormComponentIdentifier(formComponent);
    setTypeName(typeName);
  };

  const [, drop] = useDrop({
    accept: propertyType,
    hover(item: IDraggableProperty, monitor) {
      if (ref.current) {
        const dragIndex = item.index;
        const hoverIndex = index;

        if (dragIndex === hoverIndex) {
          return;
        }

        const { bottom, top } = ref.current.getBoundingClientRect();

        const verticalMiddle = (bottom - top) / 2;

        const clientOffset = monitor.getClientOffset();

        if (clientOffset) {
          const mouseVerticalPosition = clientOffset.y - top;

          if (dragIndex < hoverIndex && mouseVerticalPosition < verticalMiddle) {
            return;
          }

          if (dragIndex > hoverIndex && mouseVerticalPosition > verticalMiddle) {
            return;
          }

          setOrder(dragIndex, hoverIndex);

          item.index = hoverIndex;
        }
      }
    },
  });

  const [, drag, preview] = useDrag({
    item: { type: propertyType, index },
  });

  preview(drop(ref));

  const buttonClasses = ['btn-unigrid-action', 'icon-only', 'btn-icon', 'btn'];

  return (
    <tr ref={ref}>
      {mode === 'view' && (
        <>
          <td className='unigrid-actions'>
            <div title={localize('general.edit')} className={clsx(buttonClasses, 'icon-style-allow')} onClick={edit}>
              <i aria-hidden='true' className='icon-edit' />
              <span className='sr-only'>{localize('general.edit')}</span>
            </div>
            <div
              title={localize('general.delete')}
              className={clsx(buttonClasses, 'icon-style-critical')}
              onClick={deleteProperty}
            >
              <i aria-hidden='true' className='icon-bin' />
              <span className='sr-only'>{localize('general.delete')}</span>
            </div>
          </td>
          <td>{name}</td>
          <td>{typeName}</td>
          <td>
            {formComponentIdentifier?.startsWith('Kentico.')
              ? formComponentIdentifier.substr(8)
              : formComponentIdentifier}
          </td>
          <td>{defaultValue}</td>
          <td>{label}</td>
          <td>{tooltip}</td>
          <td>{explanationText}</td>
        </>
      )}
      {mode === 'edit' && (
        <>
          <td className='unigrid-actions'>
            <div title={localize('general.apply')} className={clsx(buttonClasses, 'icon-style-allow')} onClick={apply}>
              <i aria-hidden='true' className='icon-check-circle' />
              <span className='sr-only'>{localize('general.apply')}</span>
            </div>
            <div
              title={localize('general.cancel')}
              className={clsx(buttonClasses, 'icon-style-critical')}
              onClick={cancel}
            >
              <i aria-hidden='true' className='icon-times-circle' />
              <span className='sr-only'>{localize('general.cancel')}</span>
            </div>
            <div ref={drag} title={localize('general.dragmove')} className={clsx(buttonClasses)}>
              <i aria-hidden='true' className='icon-dots-vertical'></i>
              <span className='sr-only'>{localize('general.dragmove')}</span>
            </div>
          </td>
          <td>
            <input
              type='text'
              value={name}
              onChange={(event) => setName(event.target.value)}
              maxLength={200}
              className={clsx('form-control', styles.input)}
            />
          </td>
          <td>{typeName}</td>
          <td>
            <select
              value={formComponentIdentifier}
              onChange={(event) => setFormComponent(event.target.value)}
              className={clsx('DropDownField', 'form-control', styles.select)}
            >
              {!formComponentIdentifier && <option>{localize('general.pleaseselect')}</option>}
              {Object.keys(formComponents).map((formComponent) => (
                <option key={formComponent} value={formComponent}>
                  {formComponent.startsWith('Kentico.') ? formComponent.substr(8) : formComponent}
                </option>
              ))}
            </select>
          </td>
          <td>{defaultValue}</td>
          <td>
            <input
              type='text'
              value={label}
              onChange={(event) => setLabel(event.target.value)}
              maxLength={200}
              className={clsx('form-control', styles.input)}
            />
          </td>
          <td>
            <input
              type='text'
              value={tooltip}
              onChange={(event) => setTooltip(event.target.value)}
              maxLength={200}
              className={clsx('form-control', styles.input)}
            />
          </td>
          <td>
            <input
              type='text'
              value={explanationText}
              onChange={(event) => setExplanationText(event.target.value)}
              className={clsx('form-control', styles.input)}
            />
          </td>
        </>
      )}
      <td className='wrap-normal filling-column' />
    </tr>
  );
};
