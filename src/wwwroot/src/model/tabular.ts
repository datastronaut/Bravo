/*!
 * Bravo for Power BI
 * Copyright (c) SQLBI corp. - All rights reserved.
 * https://www.sqlbi.com
*/

export interface TabularDatabase {
    model: TabularDatabaseInfo
    measures: TabularMeasure[]
    features: AppFeature
}

export interface TabularDatabaseInfo {
    etag?:	string
    tablesCount: number
    columnsCount: number
    maxRows: number
    size: number
    unreferencedCount: number
    autoLineBreakStyle: DaxLineBreakStyle
    columns: TabularColumn[],
    tables: TabularTable[]
}
export interface TabularTable {
    name?: string
    rowsCount: number
    size: number
    features: TabularTableFeature
    featureUnsupportedReasons: TabularTableFeatureUnsupportedReason
}
export interface TabularColumn {
    columnName?: string
    tableName?: string
    columnCardinality: number
    size: number
    weight: number
    isReferenced?: boolean
}

export interface TabularMeasure {
    etag?:	string
    name?:	string
    tableName?:	string
    measure?:	string
}

export interface FormatDaxError {
    line: number
    column: number
    message?: string
}


export enum DaxLineBreakStyle {
    None = "None", 
    InitialLineBreak = "InitialLineBreak", 
    Auto = "Auto"
}

export interface FormattedMeasure extends TabularMeasure {
    errors?: FormatDaxError[]
}

export function daxMeasureName(measure: TabularMeasure): string {
    return `${measure.tableName}[${measure.name}]`;
}

export enum TabularTableFeature {
    None = 0,
    // AnalyzeModel range << 100,
    // FormatDaxPage range << 200,
    // ManageDatesPage range << 300,
    ExportData = 1 << 400,
    All = ExportData,
}

export enum TabularTableFeatureUnsupportedReason {
    None = 0,

    // AnalyzeModel range << 100,
    // FormatDaxPage range << 200,
    // ManageDatesPage range << 300,
    ExportDataNoColumns = 1 << 400,
}

export enum AppFeature {
    None = 0,

    AnalyzeModelPage = 1 << 100,
    AnalyzeModelSynchronize = 1 << 101,
    AnalyzeModelExportVpax = 1 << 102,
    AnalyzeModelAll = AnalyzeModelPage | AnalyzeModelSynchronize | AnalyzeModelExportVpax,

    FormatDaxPage = 1 << 200,
    FormatDaxSynchronize = 1 << 201,
    FormatDaxUpdateModel = 1 << 202,
    FormatDaxAll = FormatDaxPage | FormatDaxSynchronize | FormatDaxUpdateModel,

    ManageDatesPage = 1 << 300,
    ManageDatesUpdateModel = 1 << 301,
    ManageDatesAll = ManageDatesPage | ManageDatesUpdateModel,

    ExportDataPage = 1 << 400,
    ExportDataSynchronize = 1 << 401,
    ExportDataAll = ExportDataPage | ExportDataSynchronize,

    AllUpdateModel = FormatDaxUpdateModel | ManageDatesUpdateModel,
    All = AnalyzeModelAll | FormatDaxAll | ManageDatesAll | ExportDataAll,
}