# Survivors - Game Specification

> Version: 0.2
> Date: 2026-09-24
> Status: Planning / Prototype Preparation

## 1. Project Overview

### Genre

2D Top-down Survivor-like

### Project Goal

Unity와 AI를 활용하여 짧은 기간 안에 플레이 가능한 게임을 개발한다.

게임의 기획 → 개발 → QA → 수정 → 회귀 테스트 과정을 기록하여
`개발 구조를 이해하고 직접 구현할 수 있는 Game QA` 역량을 보여주는 것을 목표로 한다.

### Development Period

- 최소 목표: 5일
- 최대 기간: 14일

## 2. Core Game Loop

```text
전투 시작
→ 캐릭터 이동 및 자동 공격
→ 적 처치
→ 경험치 획득
→ 레벨업
→ 무기 특성 / 패시브 선택
→ 위 과정을 반복하며 캐릭터 성장
→ 플레이 시작 후 10분 도달
→ 일반 전투 종료 및 최종 보스 등장
→ 제한시간 안에 최종 보스 처치
→ Victory
```

플레이어가 전투 중 사망하거나 최종 보스 제한시간을 초과하면 Game Over.

## 3. Specification Index

세부 규칙은 각 시스템 문서를 단일 출처로 사용한다. 이 문서에는 같은 규칙을 중복해서 상세히 기록하지 않는다.

- [Player & Combat](Specs/PLAYER_COMBAT.md): 이동, 체력, 기본 화살, 폭탄, 피해 및 사망
- [Growth](Specs/GROWTH.md): 경험치, 레벨업, 선택지, 리롤, 패시브
- [Weapons](Specs/WEAPONS.md): 무기 6종과 무기별 특성
- [Enemies](Specs/ENEMIES.md): 일반 적 3종, 상위 등급, 스폰 및 난도 상승
- [Bosses](Specs/BOSSES.md): 중간 보스 3종과 최종 보스
- [Stage Flow](Specs/STAGE_FLOW.md): 맵, 10분 진행, 보스 전환, 점수 및 결과
- [UI & Controls](Specs/UI_CONTROLS.md): HUD, 입력, 레벨업 UI, 일시정지

## 4. MVP Scope

- 플레이어 이동 및 체력
- 가장 가까운 적 자동 탐색
- 활과 화살 기반 기본 자동 공격
- 쿨타임 기반 수동 폭탄 스킬
- 일반 적 3종과 상위 등급
- 일반 적 스폰 및 추적
- HP / Damage / Death
- 경험치 획득 및 레벨 시스템
- 레벨업 선택지와 개별 리롤
- 무기 6종, 동시 보유 4종, 무기별 특성 선택
- 패시브 8종과 단계별 성장
- 중간 보스 3종
- 10분 성장 타이머와 보스전 전환
- 3페이즈 최종 보스와 10분 보스 제한시간
- 점수, Victory, Game Over, Time Over, Restart
- 키보드 및 마우스 UI 조작
- 수동 일시정지

## 5. Out of Scope

- 멀티플레이
- 여러 스테이지
- 무한 맵 또는 절차적 맵 생성
- 영구 성장 시스템
- 복잡한 장비 시스템
- 상점
- 스토리 콘텐츠
- 업적 시스템
- 온라인 랭킹
- 게임패드 지원
- 충돌 장애물 및 장애물 길찾기

필요한 경우 MVP의 플레이 가능한 수직 슬라이스를 검증한 뒤 Backlog에서 재검토한다.

## 6. Tuning During Playtest

다음 항목은 구조를 먼저 구현하고 실제 플레이 결과를 근거로 조정한다.

- 플레이어와 적의 기본 능력치
- 무기 및 패시브의 구체적인 수치
- 폭탄 피해, 범위, 쿨타임
- 경험치 요구량과 오브젝트 경험치 값
- 일반 적 스폰 주기, 수량, 구성 비율
- 기본 / 상위 등급 능력치 차이
- 중간 보스 능력치, 보상 경험치량, 패턴 시간
- 최종 보스 능력치, 보호막 HP, 딜타임, 패턴 시간
- 맵 크기와 기술적 안전 한도
- 점수 배율

## 7. Intentionally Undecided

- 플레이어 캐릭터 설정과 세계관
- 무기 및 적의 최종 이름과 외형
- 구체적인 이펙트와 사운드
- 로컬 최고 기록 저장 기능
