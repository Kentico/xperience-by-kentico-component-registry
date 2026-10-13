import { Icon, type IconName } from '@kentico/xperience-admin-components';
import React from 'react';

// Registration attributes use legacy "icon-*" names; admin icon set uses "xp-*"
const toIconName = (iconClass: string): IconName =>
  iconClass.replace(/^icon-/, 'xp-') as IconName;

interface ComponentIconProps {
  readonly iconClass: string;
}

export const ComponentIcon = ({ iconClass }: ComponentIconProps) => (
  <span
    className="inline-flex justify-center"
    title={iconClass}
    aria-label={iconClass}
  >
    <Icon name={toIconName(iconClass)} fallbackIconName="xp-question-circle" />
  </span>
);
