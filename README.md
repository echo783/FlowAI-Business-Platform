# FlowAI Business Platform

FlowAI Business Platform은 ERP 운영개발 경험을 바탕으로 계약, 승인, 작업, 정산 업무 흐름을 관리하는 ASP.NET Core Web API 포트폴리오 프로젝트입니다.

단순 CRUD 예제가 아니라 실제 업무 시스템에서 자주 필요한 상태 흐름, 상태 변경 이력, Dashboard API, Agent API, Demo UI, 로컬 LLM 기반 요약 구조를 함께 보여주는 것을 목표로 합니다.

## 프로젝트 개요

이 프로젝트는 계약이 등록된 뒤 승인 요청과 승인 처리를 거쳐 작업으로 전환되고, 작업 완료 후 정산 요청과 검토/승인/보류/반려까지 이어지는 업무 흐름을 구현합니다.

현재 데이터는 InMemory 기반으로 동작하며, 포트폴리오 데모와 API 구조 검증에 초점을 맞추고 있습니다.

## 핵심 업무 흐름

```text
계약 등록
→ 승인 요청
→ 승인 처리
→ 작업 생성
→ 작업 진행
→ 정산 요청
→ 정산 검토 / 승인 / 보류 / 반려
```

## 주요 기능

- ASP.NET Core Web API
- 계약 등록 / 승인 요청 / 승인 / 반려
- 승인 요청 관리
- 승인 완료 시 작업 자동 생성
- 작업 시작 / 완료
- 정산 요청 / 검토 / 승인 / 보류 / 반려
- 상태 변경 이력 관리
- Dashboard Summary API
- Agent API
- API Key 인증
- Swagger / OpenAPI
- Demo Seed API
- 브라우저 Demo UI
- RuleBased Agent Provider
- Ollama Agent Provider
- Agent 응답의 `answer` + `facts` 분리 구조

## Agent 설계

Agent API는 업무 현황을 질문 형태로 조회하고 요약하기 위한 구조입니다.

- `RuleBased` Provider
  - LLM을 사용하지 않습니다.
  - 서버에서 확정한 `facts` 값을 기준으로 규칙 기반 응답을 생성합니다.
  - Ollama가 없는 환경에서 사용할 수 있는 fallback입니다.

- `Ollama` Provider
  - 로컬 Ollama LLM을 호출해 업무 현황을 자연어로 요약합니다.
  - 로컬 개발 데모 환경에서는 `appsettings.Development.json` 기준으로 Ollama Provider를 사용합니다.
  - 브라우저가 Ollama를 직접 호출하지 않고, ASP.NET Core API 서버가 Ollama와 통신합니다.

자기소개, 업무 범위 밖 질문, 단순 숫자 질문은 LLM으로 보내지 않고 C# 서버 로직에서 안전하게 응답합니다.

## answer + facts 분리

Agent 응답은 자연어 답변인 `answer`와 서버가 확정한 수치 데이터인 `facts`를 분리합니다.

이 구조의 목적은 LLM이 업무 숫자나 상태를 임의로 판단하지 않도록 하는 것입니다.

- `facts`: C# 서버 로직이 계산한 신뢰 가능한 값
- `answer`: 확정된 데이터를 바탕으로 만든 자연어 설명
- 프론트엔드 또는 외부 시스템은 핵심 수치를 `facts` 기준으로 사용할 수 있습니다.

예시:

```json
{
  "answer": "[업무 현황 요약]\n현재 승인 대기 계약은 2건이며, 오늘 진행 중인 작업은 1건입니다.",
  "source": "ollama",
  "facts": {
    "pendingApprovalContracts": 2,
    "todayWorkOrders": 1,
    "delayedWorkOrders": 0,
    "onHoldSettlements": 1,
    "totalSettlements": 5
  }
}
```

## Demo UI

Swagger 없이도 브라우저에서 주요 기능을 테스트할 수 있는 정적 Demo UI를 제공합니다.

접속 주소:

```text
http://localhost:5284
```

Demo UI에서 확인할 수 있는 기능:

- 샘플 데이터 초기화
- 업무 현황 새로고침
- 승인 대기 계약 보기
- 오늘 작업 보기
- 보류 정산 보기
- Agent에게 질문하기
- Agent 결과의 `source`, `answer`, `facts` 확인

Demo UI는 개발 및 포트폴리오 데모용이며, 로그인 기능은 포함하지 않습니다.

## 실행 방법

로컬 LLM 기반 Agent 데모를 확인하려면 Ollama 모델을 먼저 준비합니다.

```bash
ollama pull llama3.2:3b
```

API 실행:

```bash
cd backend/FlowAI.Api/FlowAI.Api
dotnet run
```

브라우저 접속:

```text
http://localhost:5284
```

로컬 개발용 API Key:

```text
local-dev-key
```

Ollama가 설치되어 있지 않은 환경에서는 `appsettings.Development.json`의 `Agent:Mode`를 `RuleBased`로 변경하면 LLM 없이 데모를 실행할 수 있습니다.

## 설정 예시

`appsettings.json`은 Ollama가 없는 환경에서도 프로젝트가 기본 실행될 수 있도록 `RuleBased`를 기본값으로 둡니다.

```json
{
  "Agent": {
    "Mode": "RuleBased",
    "OllamaBaseUrl": "http://localhost:11434",
    "OllamaModel": "llama3.2:3b"
  }
}
```

`appsettings.Development.json`은 로컬 포트폴리오 AI 데모를 위해 `Ollama`를 사용합니다.

```json
{
  "Agent": {
    "Mode": "Ollama",
    "OllamaBaseUrl": "http://localhost:11434",
    "OllamaModel": "llama3.2:3b"
  }
}
```

## API Key

모든 `/api` 요청은 API Key 헤더를 사용합니다.

```http
X-FlowAI-Api-Key: local-dev-key
```

`local-dev-key`는 로컬 개발용 샘플 키입니다. 실제 운영 환경에서는 GitHub에 비밀값을 올리지 말고 User Secrets 또는 환경변수를 사용해야 합니다.

Swagger, OpenAPI, health check, 정적 Demo UI는 API Key 보호 대상에서 제외되어 있습니다.

## OpenAPI / Swagger

API 문서는 Swagger UI와 OpenAPI JSON으로 확인할 수 있습니다.

```text
http://localhost:5284/swagger
http://localhost:5284/swagger/v1/swagger.json
```

Power Platform Custom Connector로 가져가기 위한 OpenAPI 파일은 수동으로 저장할 수 있습니다.

```text
openapi/flowai-api.json
```

현재 저장소에는 OpenAPI 2.0 변환이 필요할 수 있다는 메모 문서도 포함되어 있습니다.

```text
openapi/flowai-api-swagger2-note.md
```

## 현재 범위와 한계

- 포트폴리오 데모 목적의 프로젝트입니다.
- 데이터 저장은 InMemory 기반이며 Demo Seed API로 샘플 데이터를 생성합니다.
- 실제 운영 DB, 로그인/권한, 배포 환경은 포함하지 않습니다.
- Power Platform / Copilot Studio 실제 등록은 개인 계정 및 라이선스 제한으로 보류했습니다.
- 대신 OpenAPI와 API Key 기반으로 외부 연동 가능한 구조를 준비했습니다.
- AI 활용은 로컬 Ollama LLM 연동 기준으로 설명합니다.
- RuleBased Provider는 AI가 아니라 LLM 미사용 fallback입니다.

## 향후 확장 가능성

- SQLite 또는 SQL Server 기반 영속 저장소
- EF Core 적용
- Power Platform Custom Connector 연동
- Copilot Studio Agent 연동
- Azure 배포
- 권한 / 사용자 관리
