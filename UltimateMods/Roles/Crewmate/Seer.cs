namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Seer : RoleBase<Seer>
    {

        public static List<Vector3> DeadBodyPositions = new();

        public static float SoulDuration { get { return CustomRolesH.SeerSoulDuration.getFloat(); } }
        public static bool LimitSoulDuration { get { return CustomRolesH.SeerLimitSoulDuration.getBool(); } }
        public static int Mode { get { return CustomRolesH.SeerMode.getSelection(); } }

        private static Sprite SoulSprite;
        public static Sprite GetSoulSprite()
        {
            if (SoulSprite) return SoulSprite;
            SoulSprite = Helpers.LoadSpriteFromTexture2D(Soul, 500f);
            return SoulSprite;
        }
        public Seer()
        {
            RoleType = roleId = RoleType.Seer;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            if (Seer.DeadBodyPositions != null && PlayerControl.LocalPlayer.isRole(RoleType.Seer) && (Seer.Mode == 0 || Seer.Mode == 2))
            {
                foreach (Vector3 pos in Seer.DeadBodyPositions)
                {
                    GameObject soul = new();
                    // soul.transform.position = pos;
                    soul.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000 - 1f);
                    soul.layer = 5;
                    var rend = soul.AddComponent<SpriteRenderer>();
                    rend.sprite = Seer.GetSoulSprite();

                    if (Seer.LimitSoulDuration)
                    {
                        HudManager.Instance.StartCoroutine(Effects.Lerp(Seer.SoulDuration, new Action<float>((p) =>
                        {
                            if (rend != null)
                            {
                                var tmp = rend.color;
                                tmp.a = Mathf.Clamp01(1 - p);
                                rend.color = tmp;
                            }
                            if (p == 1f && rend != null && rend.gameObject != null) UnityEngine.Object.Destroy(rend.gameObject);
                        })));
                    }
                }
                Seer.DeadBodyPositions = new List<Vector3>();
            }
        }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            DeadBodyPositions = new();
            players = new List<Seer>();
        }
    }
}