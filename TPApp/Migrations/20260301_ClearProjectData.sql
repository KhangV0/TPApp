-- =====================================================
-- CLEAR ALL PROJECT DATA
-- Xóa theo thứ tự: bảng con → bảng cha
-- =====================================================

BEGIN TRANSACTION;

-- 1. ESign related
DELETE FROM ESignSignatures;
DELETE FROM ESignDocuments;

-- 2. Contract related
DELETE FROM ContractComments;
DELETE FROM ProjectContracts;
DELETE FROM EContracts;

-- 3. Workflow & Steps
DELETE FROM WorkflowTransitionLogs;
DELETE FROM ProjectStepStates;
DELETE FROM ProjectWorkflowStates;
DELETE FROM ProjectSteps;

-- 4. Project child tables
DELETE FROM ProjectAccessLogs;
DELETE FROM ProjectMembers;
DELETE FROM ProjectConsultants;

-- 5. Forms & Documents
DELETE FROM NDAAgreements;
DELETE FROM NegotiationForms;
DELETE FROM ProposalSubmissions;
DELETE FROM RFQInvitations;
DELETE FROM RFQRequests;
DELETE FROM LegalReviewForms;

-- 6. Implementation & Handover
DELETE FROM ImplementationLogs;
DELETE FROM AdvancePaymentConfirmations;
DELETE FROM PilotTestReports;
DELETE FROM TechTransferRequests;
DELETE FROM TrainingHandovers;
DELETE FROM TechnicalDocHandovers;
DELETE FROM HandoverReports;
DELETE FROM AcceptanceReports;
DELETE FROM LiquidationReports;

-- 7. Notifications (only project-related)
DELETE FROM Notifications WHERE ProjectId IS NOT NULL;

-- 8. Finally, the Projects table
DELETE FROM Projects;

-- Reset identity seeds
DBCC CHECKIDENT ('Projects', RESEED, 0);

COMMIT TRANSACTION;

PRINT 'All project data cleared successfully!';
