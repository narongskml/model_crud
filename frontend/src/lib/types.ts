export interface PortModelMapping {
    accnoSleeve: string;
    effectiveDate: string; // DateOnly comes as string "YYYY-MM-DD"
    modelName: string;
    currencyModel?: string;
    hedgeModelName?: string;
    isDeleted: boolean;
    createdBy?: string;
    createdAt?: string;
    updatedBy?: string;
    updatedAt?: string;
}

export interface PortModelMappingAudit {
    id: number;
    accnoSleeve: string;
    effectiveDate: string;
    modelName?: string;
    currencyModel?: string;
    hedgeModelName?: string;
    action: 'I' | 'U' | 'D';
    changedBy: string;
    changedAt: string;
}

export interface ApiWarning {
    warnings: string[];
}

export interface CreateResponse {
    record: PortModelMapping;
    warnings: string[];
}
