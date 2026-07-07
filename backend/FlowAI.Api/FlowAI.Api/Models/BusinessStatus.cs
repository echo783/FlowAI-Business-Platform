namespace FlowAI.Api.Models;

public enum BusinessStatus
{
    ContractRegistered = 10,        // 계약 등록
    ContractApprovalPending = 20,   // 계약 승인 대기
    ContractApproved = 30,          // 계약 승인

    WorkCreated = 40,               // 작업 생성
    WorkInProgress = 50,            // 작업 진행 중
    WorkCompleted = 60,             // 작업 완료

    SettlementRequested = 70,       // 정산 요청
    SettlementReviewing = 80,       // 정산 검토 중
    SettlementApproved = 90,        // 정산 승인
    SettlementHeld = 100            // 정산 보류
}