import i18n from 'i18next';
import React from 'react';
import { render } from 'react-dom';
import { initReactI18next } from 'react-i18next';

import { PropertiesEditor } from './app/PropertiesEditor';

const valueElement = document.getElementById('propertiesValue') as HTMLInputElement;

if (valueElement instanceof HTMLInputElement) {
  const resources = valueElement.dataset.localization && JSON.parse(valueElement.dataset.localization);
  const value = JSON.parse(valueElement.value);
  const formComponents = valueElement.dataset.formComponents && JSON.parse(valueElement.dataset.formComponents);
  const name = valueElement.name;

  i18n.use(initReactI18next).init({
    resources,
    fallbackLng: 'en-US',
    keySeparator: false,
    interpolation: {
      escapeValue: false,
    },
  });

  render(
    <PropertiesEditor value={value} name={name} formComponents={formComponents} />,
    document.getElementById('propertiesEditor')
  );
}
