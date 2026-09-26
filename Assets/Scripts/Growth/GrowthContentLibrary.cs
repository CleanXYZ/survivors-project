using System;
using UnityEngine;

namespace Survivors.Growth
{
    [Serializable]
    public sealed class WeaponTraitDefinition
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;

        public WeaponTraitDefinition(string id, string displayName, string description)
        {
            this.id = id;
            this.displayName = displayName;
            this.description = description;
        }
    }

    [Serializable]
    public sealed class WeaponDefinition
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private WeaponTraitDefinition[] traits;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public WeaponTraitDefinition[] Traits => traits ?? Array.Empty<WeaponTraitDefinition>();

        public WeaponDefinition(
            string id,
            string displayName,
            string description,
            params WeaponTraitDefinition[] traits)
        {
            this.id = id;
            this.displayName = displayName;
            this.description = description;
            this.traits = traits;
        }
    }

    [Serializable]
    public sealed class PassiveDefinition
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;

        public PassiveDefinition(string id, string displayName, string description)
        {
            this.id = id;
            this.displayName = displayName;
            this.description = description;
        }
    }

    public sealed class GrowthContentLibrary : MonoBehaviour
    {
        [Header("Static Growth Definitions")]
        [SerializeField] private WeaponDefinition[] weapons = CreateDefaultWeapons();
        [SerializeField] private PassiveDefinition[] passives = CreateDefaultPassives();

        public WeaponDefinition[] Weapons => weapons ?? Array.Empty<WeaponDefinition>();
        public PassiveDefinition[] Passives => passives ?? Array.Empty<PassiveDefinition>();

        private void Awake()
        {
            if (weapons == null || weapons.Length == 0)
            {
                weapons = CreateDefaultWeapons();
            }

            if (passives == null || passives.Length == 0)
            {
                passives = CreateDefaultPassives();
            }
        }

        [ContextMenu("Restore Default Growth Definitions")]
        private void RestoreDefaults()
        {
            weapons = CreateDefaultWeapons();
            passives = CreateDefaultPassives();
        }

        private static WeaponTraitDefinition Trait(
            string id,
            string displayName,
            string description)
        {
            return new WeaponTraitDefinition(id, displayName, description);
        }

        private static WeaponDefinition[] CreateDefaultWeapons()
        {
            return new[]
            {
                new WeaponDefinition(
                    "bow", "활", "가장 가까운 적에게 직선 화살을 자동 발사합니다.",
                    Trait("extra_shot", "추가 발사", "한 번에 발사하는 화살 수가 1발 증가합니다."),
                    Trait("piercing", "관통", "첫 적중 뒤 적 한 명을 추가로 관통합니다."),
                    Trait("split_shot", "분열탄", "첫 적중 시 좌우로 작은 화살 두 발이 갈라집니다."),
                    Trait("marking_shot", "표식탄", "같은 적에게 3회 적중하면 추가 단일 피해를 줍니다."),
                    Trait("explosive_shot", "폭발탄", "화살이 최종적으로 사라지는 지점에 작은 폭발을 일으킵니다."),
                    Trait("focused_fire", "집중 화력", "활과 화살의 피해량이 증가합니다."),
                    Trait("rapid_fire", "신속 발사", "활과 화살의 공격속도가 증가합니다.")),
                new WeaponDefinition(
                    "orbiting", "궤도 무기", "플레이어 주변을 공전하며 접촉한 적을 공격합니다.",
                    Trait("extra_blade", "칼날 추가", "공전하는 무기 수가 1개 증가합니다."),
                    Trait("rotation_speed", "회전 가속", "공전 속도가 증가합니다."),
                    Trait("orbit_radius", "궤도 범위 증가", "공전 반경이 증가합니다."),
                    Trait("strong_knockback", "강한 밀치기", "일반 적에게 주는 넉백이 증가합니다."),
                    Trait("shockwave", "회전 충격파", "한 바퀴마다 원형 충격파를 발생시킵니다."),
                    Trait("projectile_guard", "투사체 방어", "적 투사체를 제거하고 칼날이 잠시 비활성화됩니다."),
                    Trait("blade_power", "날 강화", "궤도 무기의 직접 피해가 증가합니다.")),
                new WeaponDefinition(
                    "returning", "왕복 무기", "적 방향으로 날아갔다가 플레이어에게 돌아옵니다.",
                    Trait("extra_throw", "추가 투척", "왕복 무기 하나를 추가로 발사합니다."),
                    Trait("flight_distance", "비행 거리 증가", "귀환 전 최대 이동 거리가 증가합니다."),
                    Trait("travel_speed", "고속 왕복", "나가는 속도와 돌아오는 속도가 증가합니다."),
                    Trait("return_damage", "귀환 강화", "돌아오는 경로의 피해가 증가합니다."),
                    Trait("turning_barrage", "회전 지점 난무", "최대 거리에서 주변 적을 연속 공격한 뒤 귀환합니다."),
                    Trait("turning_explosion", "최대 거리 폭발", "최대 거리에서 폭발한 뒤 귀환합니다."),
                    Trait("weapon_power", "무기 강화", "왕복 무기의 기본 피해가 증가합니다.")),
                new WeaponDefinition(
                    "poison_flask", "독병", "적 위치에 지속 피해와 감속을 주는 독 장판을 만듭니다.",
                    Trait("extra_throw", "추가 투척", "독병 하나를 추가로 던집니다."),
                    Trait("pool_radius", "장판 확대", "독 장판 반경이 증가합니다."),
                    Trait("duration", "지속시간 증가", "독 장판 유지시간이 증가합니다."),
                    Trait("poison_damage", "맹독", "독의 주기 피해가 증가합니다."),
                    Trait("strong_slow", "강한 감속", "독 장판의 감속률이 증가합니다."),
                    Trait("lingering_poison", "잔류 독성", "장판을 벗어나도 잠시 독 피해가 계속됩니다."),
                    Trait("corrosion", "부식", "장판 안의 적이 다른 무기에서 받는 피해도 증가합니다.")),
                new WeaponDefinition(
                    "chain_lightning", "연쇄 번개", "가장 가까운 적부터 주변의 다른 적에게 즉시 이어집니다.",
                    Trait("chain_count", "연쇄 횟수 증가", "추가로 넘어갈 수 있는 적의 수가 증가합니다."),
                    Trait("chain_range", "연쇄 거리 증가", "다음 대상을 찾는 범위가 증가합니다."),
                    Trait("forked_lightning", "갈래 번개", "최초 대상에서 두 갈래로 나뉘어 연쇄합니다."),
                    Trait("overcharge", "과충전", "남은 연쇄 횟수로 마지막 대상을 추가 타격합니다."),
                    Trait("final_discharge", "최종 방전", "마지막 대상 주변에 범위 피해를 줍니다."),
                    Trait("current_power", "전류 강화", "연쇄 번개의 피해량이 증가합니다."),
                    Trait("fast_charge", "고속 충전", "연쇄 번개의 공격속도가 증가합니다.")),
                new WeaponDefinition(
                    "spread_shot", "산탄", "가장 가까운 적 방향으로 여러 투사체를 부채꼴로 발사합니다.",
                    Trait("extra_pellet", "산탄 추가", "한 번에 발사하는 산탄 수가 증가합니다."),
                    Trait("tight_spread", "집탄 강화", "탄 퍼짐 각도가 좁아집니다."),
                    Trait("piercing_pellet", "관통 산탄", "각 산탄이 적 한 명을 추가로 관통합니다."),
                    Trait("focused_hit", "집중 타격", "여러 산탄이 같은 적에게 맞으면 추가 피해를 줍니다."),
                    Trait("strong_knockback", "강한 밀치기", "일반 적에게 주는 넉백이 증가합니다."),
                    Trait("powder_power", "화약 강화", "산탄 한 발의 피해가 증가합니다."),
                    Trait("fast_reload", "빠른 장전", "산탄형 무기의 공격속도가 증가합니다."))
            };
        }

        private static PassiveDefinition[] CreateDefaultPassives()
        {
            return new[]
            {
                new PassiveDefinition("max_health", "최대 HP 증가", "최대 HP와 현재 HP가 함께 증가합니다."),
                new PassiveDefinition("move_speed", "이동속도 증가", "플레이어의 기본 이동속도가 증가합니다."),
                new PassiveDefinition("pickup_range", "아이템 획득 범위 증가", "경험치 오브젝트의 추적 시작 범위가 증가합니다."),
                new PassiveDefinition("global_damage", "전역 무기 공격력 증가", "모든 자동 무기의 피해량이 증가합니다."),
                new PassiveDefinition("global_attack_speed", "전역 무기 공격속도 증가", "모든 자동 무기의 공격 빈도가 증가합니다."),
                new PassiveDefinition("health_regeneration", "체력 재생속도 증가", "무피격 체력 재생의 초당 회복량이 증가합니다."),
                new PassiveDefinition("experience_gain", "경험치 획득량 증가", "경험치 오브젝트가 제공하는 값이 증가합니다."),
                new PassiveDefinition("invulnerability", "피격 후 무적시간 증가", "피격 뒤 적용되는 전역 무적시간이 증가합니다.")
            };
        }
    }
}
