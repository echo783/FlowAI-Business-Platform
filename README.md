# FlowAI-Business-Platform
ASP.NET Core 기반 계약, 작업, 정산 업무 흐름 관리 포트폴리오 프로젝트

## Agent Providers

- `RuleBased`: LLM 없이 현재 업무 데이터와 규칙 기반 분기로 요약합니다. 기본 Provider입니다.
- `Ollama`: 로컬 Ollama LLM을 호출해 현재 업무 데이터를 자연어로 요약합니다.

`Agent:Mode`는 기본적으로 `RuleBased`를 사용합니다. 기존 설정 호환을 위해 `Mock` 값도 `RuleBased`와 동일하게 처리됩니다.
