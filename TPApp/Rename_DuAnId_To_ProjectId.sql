-- Rename DuAnId column to ProjectId in all workflow tables
EXEC sp_rename 'TechTransferRequests.DuAnId', 'ProjectId', 'COLUMN';
EXEC sp_rename 'NDAAgreements.DuAnId', 'ProjectId', 'COLUMN';
EXEC sp_rename 'RFQRequests.DuAnId', 'ProjectId', 'COLUMN';
EXEC sp_rename 'ProposalSubmissions.DuAnId', 'ProjectId', 'COLUMN';
EXEC sp_rename 'NegotiationForms.DuAnId', 'ProjectId', 'COLUMN';
EXEC sp_rename 'EContracts.DuAnId', 'ProjectId', 'COLUMN';
EXEC sp_rename 'AdvancePaymentConfirmations.DuAnId', 'ProjectId', 'COLUMN';
EXEC sp_rename 'ImplementationLogs.DuAnId', 'ProjectId', 'COLUMN';
EXEC sp_rename 'HandoverReports.DuAnId', 'ProjectId', 'COLUMN';
EXEC sp_rename 'AcceptanceReports.DuAnId', 'ProjectId', 'COLUMN';
EXEC sp_rename 'LiquidationReports.DuAnId', 'ProjectId', 'COLUMN';
