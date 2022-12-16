namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Adversity : RoleBase<Adversity>
    {
        public static bool isLast = false;
        public static List<CustomArrow> arrows = new();
        public static float updateTimer = 0f;
        public static float cooldown {get{return CustomRolesH.AdversityAdversityStateKillCooldown.getFloat();}}
        public static bool canFindMad {get{return CustomRolesH.AdversityAdversityStateCanFindMadmate.getBool();}}

        public Adversity()
        {
            RoleType = roleId = RoleType.Adversity;
            isLast = false;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            if (isLast)
            {
                PlayerControl.LocalPlayer.SetKillTimer(cooldown);
            }
        }
        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer.isRole(RoleType.Adversity) && isLast && canFindMad && Madmate.exists) arrowUpdate();
        }
        public override void OnKill(PlayerControl target)
        {
            if (isLast)
            {
                PlayerControl.LocalPlayer.SetKillTimer(cooldown);
            }
        }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
        public static void MakeButtons(HudManager hm) { }

        public static void SetButtonCooldowns() { }

        public static void CheckAndAdversityState()
        {
            List<PlayerControl> imps = new();
            foreach(var imp in PlayerControl.AllPlayerControls)
                if (imp.IsImpostor() && imp.IsAlive()) imps.Add(imp);

            if (imps.Count == 1) isLast = true;
        }

        static void arrowUpdate()
        {

            // 前フレームからの経過時間をマイナスする
            updateTimer -= Time.fixedDeltaTime;

            // 1秒経過したらArrowを更新
            if (updateTimer <= 0.0f)
            {

                // 前回のArrowをすべて破棄する
                foreach (CustomArrow arrow in arrows)
                {
                    if (arrow?.arrow != null)
                    {
                        arrow.arrow.SetActive(false);
                        UnityEngine.Object.Destroy(arrow.arrow);
                    }
                }

                // Arrorw一覧
                arrows = new List<CustomArrow>();

                // インポスターの位置を示すArrorwを描画
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.IsDead()) continue;
                    CustomArrow arrow;
                    if (p.isRole(RoleType.Madmate))
                    {
                        if (p.Data.Role.IsImpostor)
                        {
                            arrow = new CustomArrow(ColorDictionary.ImpostorRed);
                        }
                        else
                        {
                            arrow = new CustomArrow(Palette.Black);
                        }
                        arrow.arrow.SetActive(true);
                        arrow.Update(p.transform.position);
                        arrows.Add(arrow);
                    }
                }
            }
        }

        public override void Clear()
        {
            players = new List<Adversity>();
        }
    }
}