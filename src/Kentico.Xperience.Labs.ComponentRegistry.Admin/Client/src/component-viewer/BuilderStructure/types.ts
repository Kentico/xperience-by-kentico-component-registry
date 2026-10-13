/** Mirrors `BuilderNodeKinds`. */
export type BuilderNodeKind =
  | 'root'
  | 'template'
  | 'editableArea'
  | 'section'
  | 'zone'
  | 'widget'
  | 'variant';

/** Mirrors `BuilderPropertyDto`. */
export interface BuilderPropertyDto {
  readonly name: string;
  readonly value: string;
}

/** Mirrors `BuilderNodeDto`. */
export interface BuilderNodeDto {
  readonly kind: BuilderNodeKind;
  readonly name?: string | null;
  readonly componentIdentifier?: string | null;
  readonly componentName?: string | null;
  readonly iconClass?: string | null;
  /** `null` for nodes that are not registered components (areas, zones, variants). */
  readonly isRegistered?: boolean | null;
  readonly instanceId?: string | null;
  readonly configurationId?: string | null;
  readonly conditionType?: string | null;
  readonly properties: readonly BuilderPropertyDto[];
  readonly conditionParameters: readonly BuilderPropertyDto[];
  readonly children: readonly BuilderNodeDto[];
}

export type VersionStatus =
  'InitialDraft' | 'Draft' | 'Published' | 'Unpublished';

/** Mirrors `BuilderStructureDto`. */
export interface BuilderStructureDto {
  readonly languageName: string;
  readonly languageDisplayName: string;
  readonly versionStatus: VersionStatus;
  readonly differsFromPublished?: boolean | null;
  readonly root?: BuilderNodeDto | null;
  readonly widgetsJson?: string | null;
  readonly templateConfigurationJson?: string | null;
  readonly errors: readonly string[];
}

/** Mirrors `BuilderStructureBuilders`. */
export type BuilderType = 'pageBuilder' | 'emailBuilder';

/** Mirrors `BuilderStructureClientProperties`. */
export interface BuilderStructureClientProperties {
  readonly builder: BuilderType;
  readonly languageName: string;
  readonly structure?: BuilderStructureDto | null;
}

/** `simple` shows only the structure and identifiers, `detailed` all metadata. */
export type BuilderDisplayMode = 'simple' | 'detailed';

/** Wording that differs between Page Builder and Email Builder. */
export interface BuilderLabels {
  readonly builderName: string;
  /** Edited object, e.g. "page". */
  readonly subject: string;
  readonly rootLabel: string;
  readonly templateLabel: string;
}

export const builderLabels: Record<BuilderType, BuilderLabels> = {
  pageBuilder: {
    builderName: 'Page Builder',
    subject: 'page',
    rootLabel: 'Page',
    templateLabel: 'Page template',
  },
  emailBuilder: {
    builderName: 'Email Builder',
    subject: 'email',
    rootLabel: 'Email',
    templateLabel: 'Email template',
  },
};
