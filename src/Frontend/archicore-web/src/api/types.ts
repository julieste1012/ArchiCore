export type UserAccount = {
  id: string;
  email: string;
  displayName: string;
  role: string;
  isActive: boolean;
  createdAtUtc: string;
};

export type AuthenticationResponse = {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
  user: UserAccount;
};

export type PagedResult<T> = {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  items: T[];
};

export type CustomerContractReference = {
  contractId: string;
  contractNumber: string;
};

export type Customer = {
  id: string;
  nit: string;
  status: string;
  createdAt: string;
  updatedAt: string | null;
  contracts: CustomerContractReference[];
};

export type ContractProjectReference = {
  projectId: string;
  projectCode: string;
};

export type Contract = {
  id: string;
  contractNumber: string;
  clientId: string;
  startDate: string;
  endDate: string | null;
  totalValue: number;
  currency: string;
  status: string;
  createdAt: string;
  updatedAt: string | null;
  projects: ContractProjectReference[];
};

export type Warehouse = {
  id: string;
  warehouseCode: string;
  status: string;
  createdAt: string;
  updatedAt: string | null;
};

export type WarehouseAssignment = {
  warehouseId: string;
  assignedFrom: string;
  releasedOn: string | null;
  isActive: boolean;
};

export type Project = {
  id: string;
  projectCode: string;
  contractId: string;
  coordinatorId: string | null;
  status: string;
  startDate: string | null;
  endDate: string | null;
  createdAt: string;
  updatedAt: string | null;
  warehouseAssignments: WarehouseAssignment[];
};

export type DomainEventAuditEntry = {
  id: string;
  eventType: string;
  occurredOnUtc: string;
  recordedAtUtc: string;
  payload: string;
};

export type CreateUserRequest = {
  email: string;
  displayName: string;
  password: string;
  role: string;
};

export type ApiProblem = {
  title?: string;
  detail?: string;
  status?: number;
  code?: string;
};


export type SecurityAuditEntry = {
  id: string;
  occurredOnUtc: string;
  action: string;
  outcome: string;
  actorUserId: string | null;
  actorRole: string | null;
  targetUserId: string | null;
  targetRole: string | null;
};


export type DocumentaryReceptionStatus = 'Received' | 'Inspected' | 'CountingRecorded' | 'Counted' | 'Suspended';

export type DocumentaryReception = {
  id: string;
  contractId: string;
  customerId: string;
  receivedByUserId: string;
  receivedAtUtc: string;
  initialObservations: string | null;
  status: DocumentaryReceptionStatus;
  inspectedByUserId: string | null;
  inspectionCompletedAtUtc: string | null;
  safeToAccept: boolean | null;
  inspectionObservations: string | null;
  declaredBoxCount: number | null;
  declaredFolderCount: number | null;
  countedBoxCount: number | null;
  countedFolderCount: number | null;
  discrepancyReport: string | null;
  countedByUserId: string | null;
  countedAtUtc: string | null;
  approvedByUserId: string | null;
  approvedAtUtc: string | null;
  suspendedByUserId: string | null;
  suspendedAtUtc: string | null;
  suspensionReason: string | null;
  createdAt: string;
  updatedAt: string | null;
};


export type ExpedientRecord = { id: string; reference: string; title: string };
export type ArchiveSubseries = { id: string; code: string; name: string; expedients: ExpedientRecord[] };
export type ArchiveSeries = { id: string; code: string; name: string; subseries: ArchiveSubseries[] };
export type ArchiveSection = { id: string; code: string; name: string; series: ArchiveSeries[] };
export type ArchiveTree = { id: string; projectId: string; sections: ArchiveSection[] };
export type Box = {
  id: string; boxCode: string; barcode: string | null; status: string;
  warehouseLocationId: string | null; folderIds: string[]; createdAt: string; updatedAt: string | null;
};
export type WarehouseLocation = {
  id: string; code: string; type: 'Aisle' | 'Shelf' | 'Level' | 'Position';
  parentLocationId: string | null; capacity: number | null; occupiedCapacity: number; boxIds: string[];
};
export type FolderDocumentRecord = {
  id: string; title: string; documentDate: string | null; folios: number[];
  annexes: { title: string; sequence: number }[];
};
export type FolderRecord = {
  id: string; folderCode: string; archiveId: string; expedientId: string;
  boxId: string | null; loanId: string | null; status: string;
  documents: FolderDocumentRecord[]; createdAt: string; updatedAt: string | null;
};
export type Loan = {
  id: string; projectId: string; folderId: string; requestedByUserId: string;
  requestedAtUtc: string; dueAtUtc: string; status: string;
  authorizedByUserId: string | null; authorizedAtUtc: string | null;
  deliveredByUserId: string | null; deliveredAtUtc: string | null;
  returnedByUserId: string | null; returnedAtUtc: string | null; overdueDetectedAtUtc: string | null;
};
export type DigitizationRequest = {
  id: string; projectId: string; folderId: string; requestedByUserId: string;
  requestedAtUtc: string; status: string; startedByUserId: string | null;
  startedAtUtc: string | null; completedByUserId: string | null; completedAtUtc: string | null;
  files: { id: string; originalFileName: string; contentType: string; sizeBytes: number; sha256: string; pageCount: number }[];
};
