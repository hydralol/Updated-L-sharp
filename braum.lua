local dba = "Braum - Poro Barber"
local _ca = "raw.github.com"
local aca = "/bolqqq/BoLScripts/master/Braum%20-%20Poro%20Barber.lua?chunk=" .. math.random(1, 1000)
local bca = "https://" .. _ca .. aca
local cca = SCRIPT_PATH .. "Braum - Poro Barber.lua"
local dca = "/bolqqq/BoLScripts/master/version/Braum.version?chunk=" .. math.random(1, 1000)
function AutoupdaterMsg(b_c)
  print("<font color=\"#4CFF4C\">[" .. _G.IsLoaded .. "]:</font> <font color=\"#FFDFBF\">" .. b_c .. ".</font>")
end
if _G.BRAUMAUTOUPDATE then
  local b_c = GetWebResult(_ca, dca)
  if b_c then
    local c_c = string.match(b_c, "%d+.%d+")
    c_c = string.match(c_c and c_c or "", "%d+.%d+")
    if c_c then
      c_c = tonumber(c_c)
      if c_c > tonumber(_G.BraumVersion) then
        AutoupdaterMsg("A new version of the script is available: [" .. c_c .. "]")
        AutoupdaterMsg("The script is now updating... please don't press [F9]!")
        DelayAction(function()
          DownloadFile(bca, cca, function()
            AutoupdaterMsg("Successfully updated! (" .. _G.BraumVersion .. " -> " .. c_c .. "), Please reload (double [F9]) for the updated version!")
          end)
        end, 3)
      else
        AutoupdaterMsg("Your script is already the latest version: [" .. c_c .. "]")
        else
          AutoupdaterMsg("ERROR: No versionsinfo found! Please manually update the script or contact QQQ!")
        end
      end
    else
    end
end
local _da = {
  VPrediction = "https://raw.github.com/Hellsing/BoL/master/common/VPrediction.lua",
  SOW = "https://raw.github.com/Hellsing/BoL/master/common/SOW.lua",
  Prodiction = "https://bitbucket.org/Klokje/public-klokjes-bol-scripts/raw/ec830facccefb3b52212dba5696c08697c3c2854/Test/Prodiction/Prodiction.lua"
}
local ada = false
local bda = 0
local cda = GetCurrentEnv() and GetCurrentEnv().FILE_NAME or ""
function AfterDownload()
  bda = bda - 1
  if bda == 0 then
    ada = false
    print("<font color=\"#4CFF4C\">[" .. _G.IsLoaded .. "]:</font><font color=\"#FF7373\"> Required libraries downloaded successfully, please reload (double [F9]).</font>")
  end
end
for b_c, c_c in pairs(_da) do
  if FileExist(LIB_PATH .. b_c .. ".lua") then
    require(b_c)
  else
    ada = true
    bda = bda + 1
    print("<font color=\"#4CFF4C\">[" .. _G.IsLoaded .. "]:</font><font color=\"#FFDFBF\"> Not all required libraries are installed. Downloading: <b><u><font color=\"#73B9FF\">" .. b_c .. "</font></u></b> now! Please don't press [F9]!</font>")
    DownloadFile(c_c, LIB_PATH .. b_c .. ".lua", AfterDownload)
  end
end
if ada then
  return
end
local dda = 1050
local __b = 1600
local a_b = 60
local b_b = 0.25
local c_b = 650
local d_b = 1250
local _ab = 1200
local aab = 80
local bab = 0.3
local cab = ARGB(100, 38, 92, 255)
local dab = ARGB(100, 255, 121, 76)
local _bb = ARGB(100, 150, 255, 115)
local abb = ARGB(100, 76, 255, 76)
local bbb = "Winter's Bite"
local cbb = "Stand Behind Me"
local dbb = "Unbreakable"
local _cb = "Glacial Fissure"
local acb = {}
local bcb = {}
local ccb = minionManager(MINION_ENEMY, 500, myHero.visionPos, MINION_SORT_HEALTH_ASC)
local dcb
local _db = 0
local adb
adb = TargetSelector(TARGET_LESS_CAST_PRIORITY, 1250, DAMAGE_MAGIC, true)
adb.name = "Braum: Target"
levelSequence = {
  startQ = {
    1,
    3,
    2,
    1,
    1,
    4,
    1,
    3,
    1,
    3,
    4,
    3,
    3,
    2,
    2,
    4,
    2,
    2
  }
}
local bdb = GetEnemyHeroes()
local cdb, ddb
local __c = {}
local a_c = {}
function OnLoad()
  IgniteCheck()
  InterruptandBlockList()
  JungleNames()
  ddb = VPrediction()
  bSOW = SOW(ddb)
  AddMenu()
  _G.oldDrawCircle = rawget(_G, "DrawCircle")
  _G.DrawCircle = DrawCircle2
  PrintChat("<font color=\"#4CFF4C\">[" .. _G.IsLoaded .. "]:</font><font color=\"#FFDFBF\"> Sucessfully loaded! Version: [<u><b>" .. _G.BraumVersion .. "</b></u>]</font>")
end
function AddMenu()
  cdb = scriptConfig("Braum - Poro Barber", "Braum")
  cdb:addTS(adb)
  cdb:addSubMenu("" .. myHero.charName .. ": Key Bindings", "KeyBind")
  cdb:addSubMenu("" .. myHero.charName .. ": Extra", "Extra")
  cdb:addSubMenu("" .. myHero.charName .. ": Orbwalk", "Orbwalk")
  cdb:addSubMenu("" .. myHero.charName .. ": SBTW-Combo", "SBTW")
  cdb:addSubMenu("" .. myHero.charName .. ": Harass", "Harass")
  cdb:addSubMenu("" .. myHero.charName .. ": KillSteal", "KS")
  cdb:addSubMenu("" .. myHero.charName .. ": LaneClear", "Farm")
  cdb:addSubMenu("" .. myHero.charName .. ": JungleClear", "Jungle")
  cdb:addSubMenu("" .. myHero.charName .. ": Drawings", "Draw")
  cdb.KeyBind:addParam("SBTWKey", "SBTW-Combo Key: ", SCRIPT_PARAM_ONKEYDOWN, false, 32)
  cdb.KeyBind:addParam("HarassKey", "HarassKey: ", SCRIPT_PARAM_ONKEYDOWN, false, string.byte("C"))
  cdb.KeyBind:addParam("HarassToggleKey", "Toggle Harass: ", SCRIPT_PARAM_ONKEYTOGGLE, false, string.byte("U"))
  cdb.KeyBind:addParam("ClearKey", "Jungle- and LaneClear Key: ", SCRIPT_PARAM_ONKEYDOWN, false, string.byte("V"))
  cdb.KeyBind:addParam("EscapeKey", "EscapeKey: ", SCRIPT_PARAM_ONKEYDOWN, false, string.byte("G"))
  cdb.KeyBind:addParam("UltimateKey", "AutoAim UltimateKey: ", SCRIPT_PARAM_ONKEYDOWN, false, string.byte("T"))
  cdb.Extra:addParam("AutoLevelSkills", "Auto Level Skills (Reload Script!)", SCRIPT_PARAM_LIST, 1, {
    "No Autolevel",
    "QEWQ - R>Q>E>W"
  })
  cdb.Extra:addParam("PChoice", "Choose Prediction", SCRIPT_PARAM_LIST, 1, {
    "VPrediction",
    "Prodiction"
  })
  cdb.Extra:addSubMenu("Stand Behind Me (W) Settings: ", "wSettings")
  cdb.Extra.wSettings:addParam("JumpEnabled", "Enable AutoJump to TeamMates in Danger: ", SCRIPT_PARAM_ONOFF, true)
  cdb.Extra:addSubMenu("Unbreakable (E) Settings: ", "eSettings")
  cdb.Extra.eSettings:addParam("BlockEnabled", "Enable Blocking Projectiles with (E)", SCRIPT_PARAM_ONOFF, true)
  cdb.Extra:addSubMenu("Glacial Fissure (R) Settings: ", "rSettings")
  cdb.Extra.rSettings:addParam("InterruptSpellsR", "Enable AutoInterrupt Spells with (R)", SCRIPT_PARAM_ONOFF, true)
  bSOW:LoadToMenu(cdb.Orbwalk)
  cdb.SBTW:addParam("sbtwItems", "Use Items in Combo: ", SCRIPT_PARAM_ONOFF, true)
  cdb.SBTW:addParam("sbtwInfo", "", SCRIPT_PARAM_INFO, "")
  cdb.SBTW:addParam("sbtwInfo", "--- Choose your abilitys for SBTW ---", SCRIPT_PARAM_INFO, "")
  cdb.SBTW:addParam("sbtwQ", "Use " .. bbb .. " (Q) in Combo: ", SCRIPT_PARAM_ONOFF, true)
  cdb.SBTW:addParam("sbtwR", "Use " .. _cb .. " (R) in Combo: ", SCRIPT_PARAM_ONOFF, true)
  cdb.SBTW:addParam("sbtwRSlider", "Use (R) if more then X enemys: ", SCRIPT_PARAM_SLICE, 3, 1, 5, 0)
  cdb.Harass:addParam("harassMana", "Don't Harass if below % Mana: ", SCRIPT_PARAM_SLICE, 20, 0, 100, -1)
  cdb.Harass:addParam("harassInfo", "", SCRIPT_PARAM_INFO, "")
  cdb.Harass:addParam("harassInfo", "--- Choose your abilitys for Harass ---", SCRIPT_PARAM_INFO, "")
  cdb.Harass:addParam("harassQ", "Use " .. bbb .. " (Q) in Harass:", SCRIPT_PARAM_ONOFF, true)
  cdb.KS:addParam("Ignite", "Use Auto Ignite: ", SCRIPT_PARAM_ONOFF, false)
  cdb.Farm:addParam("farmMana", "Don't LaneClear if below % Mana: ", SCRIPT_PARAM_SLICE, 20, 0, 100, -1)
  cdb.Farm:addParam("farmInfo", "--- Choose your abilitys for LaneClear ---", SCRIPT_PARAM_INFO, "")
  cdb.Farm:addParam("farmQ", "Farm with " .. bbb .. " (Q): ", SCRIPT_PARAM_ONOFF, true)
  cdb.Jungle:addParam("jungleMana", "Don't JungleClear if below % Mana: ", SCRIPT_PARAM_SLICE, 10, 0, 100, -1)
  cdb.Jungle:addParam("jungleInfo", "--- Choose your abilitys for JungleClear ---", SCRIPT_PARAM_INFO, "")
  cdb.Jungle:addParam("jungleQ", "Clear with " .. bbb .. " (Q):", SCRIPT_PARAM_ONOFF, true)
  cdb.Draw:addParam("drawQ", "Draw (Q) Range:", SCRIPT_PARAM_ONOFF, true)
  cdb.Draw:addParam("drawW", "Draw (W) Range:", SCRIPT_PARAM_ONOFF, false)
  cdb.Draw:addParam("drawR", "Draw (R) Range:", SCRIPT_PARAM_ONOFF, true)
  cdb.Draw:addParam("drawTarget", "Draw current target: ", SCRIPT_PARAM_ONOFF, false)
  cdb.Draw:addSubMenu("LagFreeCircles: ", "LFC")
  cdb.Draw.LFC:addParam("LagFree", "Activate Lag Free Circles", SCRIPT_PARAM_ONOFF, false)
  cdb.Draw.LFC:addParam("CL", "Length before Snapping", SCRIPT_PARAM_SLICE, 350, 75, 2000, 0)
  cdb.Draw.LFC:addParam("CLinfo", "Higher length = Lower FPS Drops", SCRIPT_PARAM_INFO, "")
  cdb.Draw:addSubMenu("PermaShow: ", "PermaShow")
  cdb.Draw.PermaShow:addParam("info", "--- Reload (Double F9) if you change the settings ---", SCRIPT_PARAM_INFO, "")
  cdb.Draw.PermaShow:addParam("HarassToggleKey", "Show HarassToggleKey: ", SCRIPT_PARAM_ONOFF, true)
  cdb.Draw.PermaShow:addParam("InterruptSpellsR", "Show Interrupt Spells with (R): ", SCRIPT_PARAM_ONOFF, true)
  cdb.Draw.PermaShow:addParam("BlockEnabled", "Show Blocking Projectiles with (E): ", SCRIPT_PARAM_ONOFF, true)
  if cdb.Draw.PermaShow.HarassToggleKey then
    cdb.KeyBind:permaShow("HarassToggleKey")
  end
  if cdb.Draw.PermaShow.InterruptSpellsR then
    cdb.Extra.rSettings:permaShow("InterruptSpellsR")
  end
  if cdb.Draw.PermaShow.BlockEnabled then
    cdb.Extra.eSettings:permaShow("BlockEnabled")
  end
  cdb:addParam("Version", "Version", SCRIPT_PARAM_INFO, _G.BraumVersion)
  cdb:addParam("Author", "Author", SCRIPT_PARAM_INFO, _G.BraumAuthor)
end
function OnTick()
  if myHero.dead then
    return
  end
  adb:update()
  Target = adb.target
  Check()
  LFCfunc()
  AutoLevelMySkills()
  KeyBindings()
  if Target then
    if cdb.KS.Ignite then
      AutoIgnite(Target)
    end
    if UltimateKey then
      AimTheR(Target)
    end
  end
  if SBTWKey then
    SBTW()
  end
  if HarassKey then
    Harass()
  end
  if HarassToggleKey then
    Harass()
  end
  if ClearKey then
    LaneClear()
    JungleClear()
  end
end
function KeyBindings()
  SBTWKey = cdb.KeyBind.SBTWKey
  HarassKey = cdb.KeyBind.HarassKey
  HarassToggleKey = cdb.KeyBind.HarassToggleKey
  ClearKey = cdb.KeyBind.ClearKey
  UltimateKey = cdb.KeyBind.UltimateKey
end
function Check()
  QREADY = myHero:CanUseSpell(_Q) == READY
  WREADY = myHero:CanUseSpell(_W) == READY
  EREADY = myHero:CanUseSpell(_E) == READY
  RREADY = myHero:CanUseSpell(_R) == READY
  IREADY = dcb ~= nil and myHero:CanUseSpell(dcb) == READY
  dfgReady = dfgSlot ~= nil and myHero:CanUseSpell(dfgSlot) == READY
  hxgReady = hxgSlot ~= nil and myHero:CanUseSpell(hxgSlot) == READY
  bwcReady = bwcSlot ~= nil and myHero:CanUseSpell(bwcSlot) == READY
  botrkReady = botrkSlot ~= nil and myHero:CanUseSpell(botrkSlot) == READY
  sheenReady = sheenSlot ~= nil and myHero:CanUseSpell(sheenSlot) == READY
  lichbaneReady = lichbaneSlot ~= nil and myHero:CanUseSpell(lichbaneSlot) == READY
  trinityReady = trinitySlot ~= nil and myHero:CanUseSpell(trinitySlot) == READY
  lyandrisReady = liandrysSlot ~= nil and myHero:CanUseSpell(liandrysSlot) == READY
  tmtReady = tmtSlot ~= nil and myHero:CanUseSpell(tmtSlot) == READY
  hdrReady = hdrSlot ~= nil and myHero:CanUseSpell(hdrSlot) == READY
  youReady = youSlot ~= nil and myHero:CanUseSpell(youSlot) == READY
  dfgSlot = GetInventorySlotItem(3128)
  hxgSlot = GetInventorySlotItem(3146)
  bwcSlot = GetInventorySlotItem(3144)
  botrkSlot = GetInventorySlotItem(3153)
  sheenSlot = GetInventorySlotItem(3057)
  lichbaneSlot = GetInventorySlotItem(3100)
  trinitySlot = GetInventorySlotItem(3078)
  liandrysSlot = GetInventorySlotItem(3151)
  tmtSlot = GetInventorySlotItem(3077)
  hdrSlot = GetInventorySlotItem(3074)
  youSlot = GetInventorySlotItem(3142)
end
function UseItems()
  if not enemy then
    enemy = Target
  end
  if ValidTarget(enemy) then
    if dfgReady and GetDistance(enemy) <= 750 then
      CastSpell(dfgSlot, enemy)
    end
    if hxgReady and GetDistance(enemy) <= 700 then
      CastSpell(hxgSlot, enemy)
    end
    if bwcReady and GetDistance(enemy) <= 450 then
      CastSpell(bwcSlot, enemy)
    end
    if botrkReady and GetDistance(enemy) <= 450 then
      CastSpell(botrkSlot, enemy)
    end
    if tmtReady and GetDistance(enemy) <= 185 then
      CastSpell(tmtSlot)
    end
    if hdrReady and GetDistance(enemy) <= 185 then
      CastSpell(hdrSlot)
    end
    if youReady and GetDistance(enemy) <= 185 then
      CastSpell(youSlot)
    end
  end
end
function OnDraw()
  if myHero.dead then
    return
  end
  if QREADY and cdb.Draw.drawQ then
    DrawCircle(myHero.x, myHero.y, myHero.z, dda, cab)
  end
  if WREADY and cdb.Draw.drawW then
    DrawCircle(myHero.x, myHero.y, myHero.z, c_b, dab)
  end
  if RREADY and cdb.Draw.drawR then
    DrawCircle(myHero.x, myHero.y, myHero.z, d_b, _bb)
  end
  if Target ~= nil and cdb.Draw.drawTarget then
    DrawCircle(Target.x, Target.y, Target.z, GetDistance(Target.minBBox, Target.maxBBox) / 2, abb)
  end
end
function AimTheQ(b_c)
  b_c = b_c or Target
  if ValidTarget(b_c) and cdb.Extra.PChoice == 1 then
    local c_c, d_c, _ac = ddb:GetLineCastPosition(b_c, b_b, a_b, dda, __b, myHero, true)
    if d_c >= 2 and GetDistance(b_c) <= dda and QREADY then
      CastSpell(_Q, c_c.x, c_c.z)
    end
  elseif ValidTarget(b_c) and cdb.Extra.PChoice == 2 then
    local c_c, d_c = Prodiction.GetPrediction(b_c, dda, __b, b_b, a_b, myHero)
    local _ac = d_c.mCollision()
    if c_c and not _ac then
      CastSpell(_Q, c_c.x, c_c.z)
    end
  end
end
function AimTheRonX(b_c)
  if RREADY and ValidTarget(b_c) then
    local c_c = cdb.SBTW.sbtwRSlider
    local d_c, _ac, aac = ddb:GetLineAOECastPosition(b_c, bab, aab, d_b, _ab, myHero)
    if _ac >= 2 and c_c <= aac then
      CastSpell(_R, d_c.x, d_c.z)
    end
  end
end
function AimTheR(b_c)
  if b_c ~= nil then
    local c_c, d_c, _ac = ddb:GetLineAOECastPosition(b_c, bab, aab, d_b, _ab, myHero)
    if d_c >= 2 then
      CastSpell(_R, c_c.x, c_c.z)
    end
  end
end
function SBTW()
  if cdb.SBTW.sbtwQ then
    AimTheQ(Target)
  end
  if cdb.SBTW.sbtwR then
    AimTheRonX(Target)
  end
  if cdb.SBTW.sbtwItems then
    UseItems()
  end
end
function Harass()
  if ManaCheck(cdb.Harass.harassMana) and cdb.Harass.harassQ then
    AimTheQ(Target)
  end
end
function AutoIgnite(b_c)
  _db = dcb and getDmg("IGNITE", b_c, myHero) or 0
  if b_c.health <= _db and GetDistance(b_c) <= 600 and dcb ~= nil and IREADY then
    CastSpell(dcb, b_c)
  end
end
function IgniteCheck()
  if myHero:GetSpellData(SUMMONER_1).name:find("SummonerDot") then
    dcb = SUMMONER_1
  elseif myHero:GetSpellData(SUMMONER_2).name:find("SummonerDot") then
    dcb = SUMMONER_2
  end
end
function JungleNames()
  JungleMobNames = {
    ["YoungLizard1.1.2"] = true,
    ["YoungLizard1.1.3"] = true,
    ["YoungLizard4.1.2"] = true,
    ["YoungLizard4.1.3"] = true,
    ["wolf2.1.2"] = true,
    ["wolf2.1.3"] = true,
    ["LesserWraith3.1.2"] = true,
    ["LesserWraith3.1.3"] = true,
    ["LesserWraith3.1.4"] = true,
    ["SmallGolem5.1.1"] = true,
    ["YoungLizard7.1.2"] = true,
    ["YoungLizard7.1.3"] = true,
    ["YoungLizard10.1.2"] = true,
    ["YoungLizard10.1.3"] = true,
    ["wolf8.1.2"] = true,
    ["wolf8.1.3"] = true,
    ["LesserWraith9.1.2"] = true,
    ["LesserWraith9.1.3"] = true,
    ["LesserWraith9.1.4"] = true,
    ["SmallGolem11.1.1"] = true
  }
  FocusJungleNames = {
    ["AncientGolem1.1.1"] = true,
    ["LizardElder4.1.1"] = true,
    ["GiantWolf2.1.1"] = true,
    ["Wraith3.1.1"] = true,
    ["Golem5.1.2"] = true,
    ["GreatWraith13.1.1"] = true,
    ["AncientGolem7.1.1"] = true,
    ["LizardElder10.1.1"] = true,
    ["GiantWolf8.1.1"] = true,
    ["Wraith9.1.1"] = true,
    ["Golem11.1.2"] = true,
    ["GreatWraith14.1.1"] = true,
    ["Dragon6.1.1"] = true,
    ["Worm12.1.1"] = true
  }
  for i = 0, objManager.maxObjects do
    local b_c = objManager:getObject(i)
    if b_c ~= nil then
      if FocusJungleNames[b_c.name] then
        table.insert(bcb, b_c)
      elseif JungleMobNames[b_c.name] then
        table.insert(acb, b_c)
      end
    end
  end
end
function JungleClear()
  JungleMob = GetJungleMob()
  if JungleMob ~= nil and ManaCheck(cdb.Jungle.jungleMana) and cdb.Jungle.jungleQ then
    AimTheQ(JungleMob)
  end
end
function GetJungleMob()
  for b_c, c_c in pairs(bcb) do
    if ValidTarget(c_c, c_b) then
      return c_c
    end
  end
  for b_c, c_c in pairs(acb) do
    if ValidTarget(c_c, c_b) then
      return c_c
    end
  end
end
function LaneClear()
  ccb:update()
  for b_c, c_c in pairs(ccb.objects) do
    if ValidTarget(c_c) and c_c ~= nil and not bSOW:CanAttack() and ManaCheck(cdb.Farm.farmMana) and cdb.Farm.farmQ then
      AimTheQ(c_c)
    end
  end
end
function OnCreateObj(b_c)
  if b_c ~= nil then
    if b_c.name:find("TeleportHome.troy") and GetDistance(b_c) <= 70 then
      Recalling = true
    end
    if FocusJungleNames[b_c.name] then
      table.insert(bcb, b_c)
    elseif JungleMobNames[b_c.name] then
      table.insert(acb, b_c)
    end
  end
end
function OnDeleteObj(b_c)
  if b_c ~= nil then
    if b_c.name:find("TeleportHome.troy") and GetDistance(b_c) <= 70 then
      Recalling = false
    end
    for c_c, d_c in pairs(acb) do
      if b_c.name == d_c.name then
        table.remove(acb, c_c)
      end
    end
    for c_c, d_c in pairs(bcb) do
      if b_c.name == d_c.name then
        table.remove(bcb, c_c)
      end
    end
  end
end
function OnRecall(b_c, c_c)
  if b_c.networkID == player.networkID then
    Recalling = true
  end
end
function OnAbortRecall(b_c)
  if b_c.networkID == player.networkID then
    Recalling = false
  end
end
function OnFinishRecall(b_c)
  if b_c.networkID == player.networkID then
    Recalling = false
  end
end
function LFCfunc()
  if not cdb.Draw.LFC.LagFree then
    _G.DrawCircle = _G.oldDrawCircle
  end
  if cdb.Draw.LFC.LagFree then
    _G.DrawCircle = DrawCircle2
  end
end
function DrawCircleNextLvl(b_c, c_c, d_c, _ac, aac, bac, cac)
  _ac = _ac or 300
  quality = math.max(8, round(180 / math.deg((math.asin(cac / (2 * _ac))))))
  quality = 2 * math.pi / quality
  _ac = _ac * 0.92
  local dac = {}
  for theta = 0, 2 * math.pi + quality, quality do
    local _bc = WorldToScreen(D3DXVECTOR3(b_c + _ac * math.cos(theta), c_c, d_c - _ac * math.sin(theta)))
    dac[#dac + 1] = D3DXVECTOR2(_bc.x, _bc.y)
  end
  DrawLines2(dac, aac or 1, bac or 4294967295)
end
function round(b_c)
  if b_c >= 0 then
    return math.floor(b_c + 0.5)
  else
    return math.ceil(b_c - 0.5)
  end
end
function DrawCircle2(b_c, c_c, d_c, _ac, aac)
  local bac = Vector(b_c, c_c, d_c)
  local cac = Vector(cameraPos.x, cameraPos.y, cameraPos.z)
  local dac = bac - (bac - cac):normalized() * _ac
  local _bc = WorldToScreen(D3DXVECTOR3(dac.x, dac.y, dac.z))
  if OnScreen({
    x = _bc.x,
    y = _bc.y
  }, {
    x = _bc.x,
    y = _bc.y
  }) then
    DrawCircleNextLvl(b_c, c_c, d_c, _ac, 1, aac, cdb.Draw.LFC.CL)
  end
end
function AutoLevelMySkills()
  if cdb.Extra.AutoLevelSkills == 2 then
    autoLevelSetSequence(levelSequence.startQ)
  end
end
function ManaCheck(b_c)
  if myHero.mana > myHero.maxMana * (b_c / 100) then
    return true
  else
    return false
  end
end
function OnProcessSpell(b_c, c_c)
  if cdb.Extra.eSettings.BlockEnabled and c_c and b_c and GetDistance(b_c) < 2000 and b_c.type == "AIHeroClient" and b_c.team == TEAM_ENEMY and a_c[c_c.name] then
    if a_c[c_c.name].Blockable then
      if a_c[c_c.name].SpellType == "skillshot" then
        local d_c, _ac, aac, bac, cac = a_c[c_c.name].delay, a_c[c_c.name].width, a_c[c_c.name].range, a_c[c_c.name].speed, a_c[c_c.name].collision
        local dac, _bc, abc = ddb:GetLineCastPosition(myHero, d_c, _ac, aac, bac, b_c, cac)
        if _bc >= 2 then
          if not _G.Evadeee and EREADY then
            CastSpell(_E, c_c.startPos.x, c_c.startPos.z)
          end
          if _G.Evadeee and _G.Evadeee_impossibleToEvade and EREADY then
            CastSpell(_E, c_c.startPos.x, c_c.startPos.z)
          end
        end
      end
    elseif a_c[c_c.name].SpellType == "enemyCast" and c_c.target.name == myHero.name then
      CastSpell(_E, c_c.startPos.x, c_c.startPos.z)
    end
  end
  if cdb.Extra.wSettings.JumpEnabled then
    if c_c and b_c and GetDistance(b_c) < 2000 and b_c.type == "obj_AI_Hero" and b_c.team == TEAM_ENEMY and a_c[c_c.name] and (a_c[c_c.name].Blockable and a_c[c_c.name].riskLevel == "extreme" or a_c[c_c.name].riskLevel == "kill") then
      local d_c = GetAllyHeroes()
      for _ac, aac in pairs(d_c) do
        if a_c[c_c.name].SpellType == "skillshot" then
          local bac, cac, dac, _bc, abc = a_c[c_c.name].delay, a_c[c_c.name].width, a_c[c_c.name].range, a_c[c_c.name].speed, a_c[c_c.name].collision
          local bbc, cbc, dbc = ddb:GetLineCastPosition(aac, bac, cac, dac, _bc, b_c, abc)
          local _cc = Point(c_c.endPos.x, c_c.endPos.z)
          if cbc >= 2 and GetDistance(aac) < c_b and aac.charName ~= myHero.charName then
            CastSpell(_W, aac)
            if EREADY then
              CastSpell(_E, c_c.startPos.x, c_c.startPos.z)
            end
          end
        end
        if a_c[c_c.name].SpellType == "enemyCast" and c_c.target.name == aac.name then
          CastSpell(_W, aac)
          if EREADY then
            CastSpell(_E, c_c.startPos.x, c_c.startPos.z)
          end
        end
      end
    end
    if cdb.Extra.rSettings.InterruptSpellsR and GetDistance(b_c) <= d_b and b_c.valid and b_c.team == TEAM_ENEMY and __c[c_c.name] then
      AimTheR(b_c)
    end
    if cdb.Extra.rSettings.InterruptSpellsR and GetDistance(b_c) <= d_b and b_c.valid and b_c.team == TEAM_ENEMY and __c[c_c.name] then
      AimTheR(b_c)
    end
  end
end
function InterruptandBlockList()
  __c = {
    AbsoluteZero = true,
    AlZaharNetherGrasp = true,
    CaitlynAceintheHole = true,
    Crowstorm = true,
    FallenOne = true,
    GalioIdolOfDurand = true,
    InfiniteDuress = true,
    KatarinaR = true,
    MissFortuneBulletTime = true,
    Teleport = true,
    Pantheon_GrandSkyfall_Jump = true,
    ShenStandUnited = true,
    UrgotSwap2 = true
  }
  a_c = {
    ["AatroxQ"] = {
      charName = "Aatrox",
      spellSlot = "Q",
      range = 650,
      width = 0,
      speed = 20,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = true,
      hitLineCheck = false
    },
    ["AatroxE"] = {
      charName = "Aatrox",
      spellSlot = "E",
      range = 1000,
      width = 150,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["AatroxR"] = {
      charName = "Aatrox",
      spellSlot = "R",
      range = 550,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      collision = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["AhriOrbofDeception"] = {
      charName = "Ahri",
      spellSlot = "Q",
      range = 880,
      width = 100,
      speed = 1100,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["AhriFoxFire"] = {
      charName = "Ahri",
      spellSlot = "W",
      range = 800,
      width = 0,
      speed = 1800,
      delay = 0,
      SpellType = "selfCast",
      collision = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["AhriSeduce"] = {
      charName = "Ahri",
      spellSlot = "E",
      range = 975,
      width = 60,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["AhriTumble"] = {
      charName = "Ahri",
      spellSlot = "R",
      range = 450,
      width = 0,
      speed = 2200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["AkaliMota"] = {
      charName = "Akali",
      spellSlot = "Q",
      range = 600,
      width = 0,
      speed = 1000,
      delay = 0.65,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["AkaliSmokeBomb"] = {
      charName = "Akali",
      spellSlot = "W",
      range = 700,
      width = 0,
      speed = 0,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["AkaliShadowSwipe"] = {
      charName = "Akali",
      spellSlot = "E",
      range = 325,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["AkaliShadowDance"] = {
      charName = "Akali",
      spellSlot = "R",
      range = 800,
      width = 0,
      speed = 2200,
      delay = 0,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["Pulverize"] = {
      charName = "Alistar",
      spellSlot = "Q",
      range = 365,
      width = 0,
      speed = 20,
      delay = 0.5,
      SpellType = "enemyCast",
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["Headbutt"] = {
      charName = "Alistar",
      spellSlot = "W",
      range = 100,
      width = 0,
      speed = 0,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["TriumphantRoar"] = {
      charName = "Alistar",
      spellSlot = "E",
      range = 575,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      healSlot = _E
    },
    ["FerouciousHowl"] = {
      charName = "Alistar",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = 828,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["BandageToss"] = {
      charName = "Amumu",
      spellSlot = "Q",
      range = 1100,
      width = 80,
      speed = 2000,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["AuraofDespair"] = {
      charName = "Amumu",
      spellSlot = "W",
      range = 300,
      width = 0,
      speed = math.huge,
      delay = 0.47,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["Tantrum"] = {
      charName = "Amumu",
      spellSlot = "E",
      range = 350,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["CurseoftheSadMumm"] = {
      charName = "Amumu",
      spellSlot = "R",
      range = 550,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false,
      timer = 0
    },
    ["FlashFrost"] = {
      charName = "Anivia",
      spellSlot = "Q",
      range = 1200,
      width = 110,
      speed = 850,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["Crystalize"] = {
      charName = "Anivia",
      spellSlot = "W",
      range = 1000,
      width = 400,
      speed = 1600,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["Frostbite"] = {
      charName = "Anivia",
      spellSlot = "E",
      range = 650,
      width = 0,
      speed = 1200,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["GlacialStorm"] = {
      charName = "Anivia",
      spellSlot = "R",
      range = 675,
      width = 400,
      speed = math.huge,
      delay = 0.3,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["Disintegrate"] = {
      charName = "Annie",
      spellSlot = "Q",
      range = 710,
      width = 0,
      speed = 1400,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "Kill",
      cc = false,
      hitLineCheck = false
    },
    ["Incinerate"] = {
      charName = "Annie",
      spellSlot = "W",
      range = 210,
      width = 0,
      speed = 0,
      delay = 0.5,
      SpellType = "enemyCast",
      collision = false,
      riskLevel = "Kill",
      cc = false,
      hitLineCheck = true
    },
    ["MoltenShield"] = {
      charName = "Annie",
      spellSlot = "E",
      range = 100,
      width = 0,
      speed = 20,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      rickLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["InfernalGuardian"] = {
      charName = "Annie",
      spellSlot = "R",
      range = 250,
      width = 0,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "Kill",
      cc = false,
      hitLineCheck = true,
      timer = 0
    },
    ["FrostShot"] = {
      charName = "Ashe",
      spellSlot = "Q",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["frostarrow"] = {
      charName = "Ashe",
      spellSlot = "Q",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["Volley"] = {
      charName = "Ashe",
      spellSlot = "W",
      range = 1200,
      width = 250,
      speed = 902,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["AsheSpiritOfTheHawk"] = {
      charName = "Ashe",
      spellSlot = "E",
      range = 2500,
      width = 0,
      speed = 1400,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      collision = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["EnchantedCrystalArrow"] = {
      charName = "Ashe",
      spellSlot = "R",
      range = 50000,
      width = 130,
      speed = 1600,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["RocketGrabMissile"] = {
      charName = "Blitzcrank",
      spellSlot = "Q",
      range = 925,
      width = 70,
      speed = 1800,
      delay = 0.22,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["Overdrive"] = {
      charName = "Blitzcrank",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["PowerFist"] = {
      charName = "Blitzcrank",
      spellSlot = "E",
      range = 0,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["StaticField"] = {
      charName = "Blitzcrank",
      spellSlot = "R",
      range = 600,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["BrandBlaze"] = {
      charName = "Brand",
      spellSlot = "Q",
      range = 1050,
      width = 80,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["BrandFissure"] = {
      charName = "Brand",
      spellSlot = "W",
      range = 240,
      width = 0,
      speed = 20,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["BrandConflagration"] = {
      charName = "Brand",
      spellSlot = "E",
      range = 0,
      width = 0,
      speed = 1800,
      delay = 0,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["BrandWildfire"] = {
      charName = "Brand",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = 1000,
      delay = 0,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false,
      timer = 230 - GetLatency()
    },
    ["BraumQ"] = {
      charName = "Braum",
      spellSlot = "Q",
      range = 1100,
      width = 100,
      speed = 1200,
      delay = 0.5,
      spellType = "skillShot",
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["BraumQMissle"] = {
      charName = "Braum",
      spellSlot = "Q",
      range = 1100,
      width = 100,
      speed = 1200,
      delay = 0.5,
      spellType = "skillShot",
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["BraumW"] = {
      charName = "Braum",
      spellSlot = "W",
      range = 650,
      width = 0,
      speed = 1500,
      delay = 0.5,
      spellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["BraumE"] = {
      charName = "Braum",
      spellSlot = "E",
      range = 250,
      width = 0,
      speed = math.huge,
      delay = 0,
      spellType = "skillshot",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["BraumR"] = {
      charName = "Braum",
      spellSlot = "R",
      range = 1250,
      width = 180,
      speed = 1200,
      delay = 0,
      spellType = "skillshot",
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["CaitlynPiltoverPeacemaker"] = {
      charName = "Caitlyn",
      spellSlot = "Q",
      range = 1250,
      width = 90,
      speed = 2200,
      delay = 0.25,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["CaitlynYordleTrap"] = {
      charName = "Caitlyn",
      spellSlot = "W",
      range = 800,
      width = 0,
      speed = 1400,
      delay = 0,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["CaitlynEntrapment"] = {
      charName = "Caitlyn",
      spellSlot = "E",
      range = 950,
      width = 80,
      speed = 2000,
      delay = 0.25,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["CaitlynAceintheHole"] = {
      charName = "Caitlyn",
      spellSlot = "R",
      range = 2500,
      width = 0,
      speed = 1500,
      delay = 0,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false,
      timer = 1350 - GetLatency()
    },
    ["CassiopeiaNoxiousBlast"] = {
      charName = "Cassiopeia",
      spellSlot = "Q",
      range = 925,
      width = 130,
      speed = math.huge,
      delay = 0.25,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["CassiopeiaMiasma"] = {
      charName = "Cassiopeia",
      spellSlot = "W",
      range = 925,
      width = 212,
      speed = 2500,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["CassiopeiaTwinFang"] = {
      charName = "Cassiopeia",
      spellSlot = "E",
      range = 700,
      width = 0,
      speed = 1900,
      delay = 0,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["CassiopeiaPetrifyingGaze"] = {
      charName = "Cassiopeia",
      spellSlot = "R",
      range = 875,
      width = 210,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true,
      timer = 0
    },
    ["Rupture"] = {
      charName = "Chogath",
      spellSlot = "Q",
      range = 1000,
      width = 250,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["FeralScream"] = {
      charName = "Chogath",
      spellSlot = "W",
      range = 675,
      width = 210,
      speed = math.huge,
      delay = 0.25,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["VorpalSpikes"] = {
      charName = "Chogath",
      spellSlot = "E",
      range = 0,
      width = 170,
      speed = 347,
      delay = 0,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["Feast"] = {
      charName = "Chogath",
      spellSlot = "R",
      range = 230,
      width = 0,
      speed = 500,
      delay = 0,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["PhosphorusBomb"] = {
      charName = "Corki",
      spellSlot = "Q",
      range = 875,
      width = 250,
      speed = math.huge,
      delay = 0,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["CarpetBomb"] = {
      charName = "Corki",
      spellSlot = "W",
      range = 875,
      width = 160,
      speed = 700,
      delay = 0,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["GGun"] = {
      charName = "Corki",
      spellSlot = "E",
      range = 750,
      width = 100,
      speed = 902,
      delay = 0,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["MissileBarrage"] = {
      charName = "Corki",
      spellSlot = "R",
      range = 1225,
      width = 40,
      speed = 828.5,
      delay = 0.25,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["DariusCleave"] = {
      charName = "Darius",
      spellSlot = "Q",
      range = 425,
      width = 0,
      speed = 0,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["DariusNoxianTacticsONH"] = {
      charName = "Darius",
      spellSlot = "W",
      range = 210,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["DariusAxeGrabCone"] = {
      charName = "Darius",
      spellSlot = "E",
      range = 540,
      speed = 1500,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = true
    },
    ["DariusExecute"] = {
      charName = "Darius",
      spellSlot = "R",
      range = 460,
      width = 0,
      speed = 20,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["DianaArc"] = {
      charName = "Diana",
      spellSlot = "Q",
      range = 900,
      width = 75,
      speed = 1500,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      collision = false,
      riskLevel = "kill",
      cc = true,
      hitLineCheck = true
    },
    ["DianaOrbs"] = {
      charName = "Diana",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false,
      shieldSlot = _W
    },
    ["DianaVortex"] = {
      charName = "Diana",
      spellSlot = "E",
      range = 300,
      width = 0,
      speed = 1500,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["DianaTeleport"] = {
      charName = "Diana",
      spellSlot = "R",
      range = 800,
      width = 0,
      speed = 1500,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = true,
      hitLineCheck = false
    },
    ["InfectedCleaverMissileCast"] = {
      charName = "DrMundo",
      spellSlot = "Q",
      range = 900,
      width = 75,
      speed = 1500,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["BurningAgony"] = {
      charName = "DrMundo",
      spellSlot = "W",
      range = 325,
      width = 0,
      speed = math.huge,
      delay = math.huge,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["Masochism"] = {
      charName = "DrMundo",
      spellSlot = "E",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = math.huge,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["Sadism"] = {
      charName = "DrMundo",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = math.huge,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["dravenspinning"] = {
      charName = "Draven",
      spellSlot = "Q",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = math.huge,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["DravenFury"] = {
      charName = "Draven",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = math.huge,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["DravenDoubleShot"] = {
      charName = "Draven",
      spellSlot = "E",
      range = 1050,
      width = 130,
      speed = 1600,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["DravenRCast"] = {
      charName = "Draven",
      spellSlot = "R",
      range = 20000,
      width = 160,
      speed = 2000,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["EliseHumanQ"] = {
      charName = "Elise",
      spellSlot = "Q",
      range = 625,
      width = 0,
      speed = 2200,
      delay = 0.75,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["EliseHumanW"] = {
      charName = "Elise",
      spellSlot = "W",
      range = 950,
      width = 235,
      speed = 5000,
      delay = 0.75,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["EliseHumanE"] = {
      charName = "Elise",
      spellSlot = "E",
      range = 1075,
      width = 70,
      speed = 1450,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = true
    },
    ["EliseR"] = {
      charName = "Elise",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = math.huge,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["EliseSpiderQCast"] = {
      charName = "Elise",
      spellSlot = "Q",
      range = 475,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["EliseSpiderW"] = {
      charName = "Elise",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = math.huge,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["EliseSpiderEInitial"] = {
      charName = "Elise",
      spellSlot = "E",
      range = 975,
      width = 0,
      speed = math.huge,
      delay = math.huge,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["elisespideredescent"] = {
      charName = "Elise",
      spellSlot = "E",
      range = 975,
      width = 0,
      speed = math.huge,
      delay = math.huge,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["EliseSpiderR"] = {
      charName = "Elise",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = math.huge,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["EvelynnQ"] = {
      charName = "Evelynn",
      spellSlot = "Q",
      range = 500,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["EvelynnW"] = {
      charName = "Evelynn",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = math.huge,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["EvelynnE"] = {
      charName = "Evelynn",
      spellSlot = "E",
      range = 290,
      width = 0,
      speed = 900,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["EvelynnR"] = {
      charName = "Evelynn",
      spellSlot = "R",
      range = 650,
      width = 350,
      speed = 1300,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["EzrealMysticShot"] = {
      charName = "Ezreal",
      spellSlot = "Q",
      range = 1150,
      width = 80,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["EzrealEssenceFlux"] = {
      charName = "Ezreal",
      spellSlot = "W",
      range = 1000,
      width = 80,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["EzrealArcaneShift"] = {
      charName = "Ezreal",
      spellSlot = "E",
      range = 475,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["EzrealTruehotBarrage"] = {
      charName = "Ezreal",
      spellSlot = "R",
      range = 20000,
      width = 160,
      speed = 2000,
      delay = 1,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["Terrify"] = {
      charName = "FiddleSticks",
      spellSlot = "Q",
      range = 575,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["Drain"] = {
      charName = "FiddleSticks",
      spellSlot = "W",
      range = 575,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["FiddlesticksDarkWind"] = {
      charName = "FiddleSticks",
      spellSlot = "E",
      range = 750,
      width = 0,
      speed = 1100,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["Crowstorm"] = {
      charName = "FiddleSticks",
      spellSlot = "R",
      range = 800,
      width = 600,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["FioraQ"] = {
      charName = "Fiora",
      spellSlot = "Q",
      range = 300,
      width = 0,
      speed = 2200,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["FioraRiposte"] = {
      charName = "Fiora",
      spellSlot = "W",
      range = 100,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      aaShieldSlot = _W
    },
    ["FioraFlurry"] = {
      charName = "Fiora",
      spellSlot = "E",
      range = 210,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["FioraDance"] = {
      charName = "Fiora",
      spellSlot = "R",
      range = 210,
      width = 0,
      speed = 0,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false,
      timer = 280 - GetLatency()
    },
    ["FizzPiercingStrike"] = {
      charName = "Fizz",
      spellSlot = "Q",
      range = 550,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["FizzSeastonePassive"] = {
      charName = "Fizz",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = 0,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["FizzJump"] = {
      charName = "Fizz",
      spellSlot = "E",
      range = 400,
      width = 120,
      speed = 1300,
      delay = 0.5,
      SpellType = "selfcast",
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false,
      shieldSlot = _E
    },
    ["FizzJumptwo"] = {
      charName = "Fizz",
      spellSlot = "E",
      range = 400,
      width = 500,
      speed = 1300,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["FizzMarinerDoom"] = {
      charName = "Fizz",
      spellSlot = "R",
      range = 1275,
      width = 250,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["GalioResoluteSmite"] = {
      charName = "Galio",
      spellSlot = "Q",
      range = 940,
      width = 120,
      speed = 1300,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["GalioBulwark"] = {
      charName = "Galio",
      spellSlot = "W",
      range = 800,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      shieldSlot = _W
    },
    ["GalioRighteousGust"] = {
      charName = "Galio",
      spellSlot = "E",
      range = 1180,
      width = 140,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["GalioIdolOfDurand"] = {
      charName = "Galio",
      spellSlot = "R",
      range = 560,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false,
      timer = 0
    },
    ["Parley"] = {
      charName = "Gangplank",
      spellSlot = "Q",
      range = 625,
      width = 0,
      speed = 2000,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["RemoveScurvy"] = {
      charName = "Gangplank",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      healSlot = _W,
      qssSlot = _W
    },
    ["RaiseMorale"] = {
      charName = "Gangplank",
      spellSlot = "E",
      range = 1300,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["CannonBarrage"] = {
      charName = "Gangplank",
      spellSlot = "R",
      range = 20000,
      width = 525,
      speed = 500,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = true,
      hitLineCheck = false
    },
    ["GarenQ"] = {
      charName = "Garen",
      spellSlot = "Q",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.2,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["GarenW"] = {
      charName = "Garen",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["GarenE"] = {
      charName = "Garen",
      spellSlot = "E",
      range = 325,
      width = 0,
      speed = 700,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["GarenR"] = {
      charName = "Garen",
      spellSlot = "R",
      range = 400,
      width = 0,
      speed = math.huge,
      delay = 0.12,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["GragasBarrelRoll"] = {
      charName = "Gragas",
      spellSlot = "Q",
      range = 1100,
      width = 320,
      speed = 1000,
      delay = 0.3,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["gragasbarrelrolltoggle"] = {
      charName = "Gragas",
      spellSlot = "Q",
      range = 1100,
      width = 320,
      speed = 1000,
      delay = 0.3,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["GragasDrunkenRage"] = {
      charName = "Gragas",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["GragasBodySlam"] = {
      charName = "Gragas",
      spellSlot = "E",
      range = 1100,
      width = 50,
      speed = 1000,
      delay = 0.3,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["GragasExplosiveCask"] = {
      charName = "Gragas",
      spellSlot = "R",
      range = 1100,
      width = 700,
      speed = 1000,
      delay = 0.3,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["GravesClusterShot"] = {
      charName = "Graves",
      spellSlot = "Q",
      range = 1100,
      width = 10,
      speed = 902,
      delay = 0.3,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["GravesSmokeGrenade"] = {
      charName = "Graves",
      spellSlot = "W",
      range = 1100,
      width = 250,
      speed = 1650,
      delay = 0.3,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["gravessmokegrenadeboom"] = {
      charName = "Graves",
      spellSlot = "W",
      range = 1100,
      width = 250,
      speed = 1650,
      delay = 0.3,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["GravesMove"] = {
      charName = "Graves",
      spellSlot = "E",
      range = 425,
      width = 50,
      speed = 1000,
      delay = 0.3,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["GravesChargeShot"] = {
      charName = "Graves",
      spellSlot = "R",
      range = 1000,
      width = 100,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["HecarimRapidSlash"] = {
      charName = "Hecarim",
      spellSlot = "Q",
      range = 350,
      width = 0,
      speed = 1450,
      delay = 0.3,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["HecarimW"] = {
      charName = "Hecarim",
      spellSlot = "W",
      range = 525,
      width = 0,
      speed = 828.5,
      delay = 0.12,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["HecarimRamp"] = {
      charName = "Hecarim",
      spellSlot = "E",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = math.huge,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["HecarimUlt"] = {
      charName = "Hecarim",
      spellSlot = "R",
      range = 1350,
      width = 200,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["HeimerdingerQ"] = {
      charName = "Heimerdinger",
      spellSlot = "Q",
      range = 350,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["HeimerdingerW"] = {
      charName = "Heimerdinger",
      spellSlot = "W",
      range = 1525,
      width = 200,
      speed = 902,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["HeimerdingerE"] = {
      charName = "Heimerdinger",
      spellSlot = "E",
      range = 970,
      width = 120,
      speed = 2500,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["HeimerdingerR"] = {
      charName = "Heimerdinger",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.23,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["IreliaGatotsu"] = {
      charName = "Irelia",
      spellSlot = "Q",
      range = 650,
      width = 0,
      speed = 2200,
      delay = 0,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["IreliaHitenStyle"] = {
      charName = "Irelia",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = 347,
      delay = 0.23,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["IreliaEquilibriumStrike"] = {
      charName = "Irelia",
      spellSlot = "E",
      range = 325,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["IreliaTranscendentBlades"] = {
      charName = "Irelia",
      spellSlot = "R",
      range = 1200,
      width = 0,
      speed = 779,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["HowlingGale"] = {
      charName = "Janna",
      spellSlot = "Q",
      range = 1800,
      width = 200,
      speed = math.huge,
      delay = 0,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["SowTheWind"] = {
      charName = "Janna",
      spellSlot = "W",
      range = 600,
      width = 0,
      speed = 1600,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["EyeOfTheStorm"] = {
      charName = "Janna",
      spellSlot = "E",
      range = 800,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      shieldSlot = _E
    },
    ["ReapTheWhirlwind"] = {
      charName = "Janna",
      spellSlot = "R",
      range = 725,
      width = 0,
      speed = 828.5,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = true,
      hitLineCheck = false
    },
    ["JarvanIVDragonStrike"] = {
      charName = "JarvanIV",
      spellSlot = "Q",
      range = 700,
      width = 70,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["JarvanIVGoldenAegis"] = {
      charName = "JarvanIV",
      spellSlot = "W",
      range = 300,
      width = 0,
      speed = 0,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false,
      shieldSlot = _W
    },
    ["JarvanIVDemacianStandard"] = {
      charName = "JarvanIV",
      spellSlot = "E",
      range = 830,
      width = 75,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["JarvanIVCataclysm"] = {
      charName = "JarvanIV",
      spellSlot = "R",
      range = 650,
      width = 325,
      speed = 0,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["JaxLeapStrike"] = {
      charName = "Jax",
      spellSlot = "Q",
      range = 210,
      width = 0,
      speed = 0,
      delay = 0.5,
      SpellType = "everyCast",
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["JaxEmpowerTwo"] = {
      charName = "Jax",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = 0,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["JaxCounterStrike"] = {
      charName = "Jax",
      spellslot = "E",
      range = 300,
      width = 0,
      speed = 1450,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["JaxRelentlessAsssault"] = {
      charName = "Jax",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["JayceToTheSkies"] = {
      charName = "Jayce",
      spellSlot = "Q",
      range = 600,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["JayceStaticField"] = {
      charName = "Jayce",
      spellSlot = "W",
      range = 285,
      width = 200,
      speed = 1500,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["JayceThunderingBlow"] = {
      charName = "Jayce",
      spellSlot = "E",
      range = 300,
      width = 80,
      speed = math.huge,
      delay = 0,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["JayceStanceHtG"] = {
      charName = "Jayce",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.75,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["jayceshockblast"] = {
      charName = "Jayce",
      spellSlot = "Q",
      range = 1050,
      width = 80,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["jaycehypercharge"] = {
      charName = "Jayce",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.75,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["jayceaccelerationgate"] = {
      charName = "Jayce",
      spellSlot = "E",
      range = 685,
      width = 0,
      speed = 1600,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["jaycestancegth"] = {
      charName = "Jayce",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.75,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["JinxW"] = {
      charName = "Jinx",
      spellSlot = "W",
      range = 1450,
      width = 80,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["JinxRWrapper"] = {
      charName = "Jinx",
      spellSlot = "R",
      range = 20000,
      width = 120,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["LayWaste"] = {
      charName = "Karthus",
      spellSlot = "Q",
      range = 875,
      width = 160,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["WallOfPain"] = {
      charName = "Karthus",
      spellSlot = "W",
      range = 1090,
      width = 525,
      speed = 1600,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["Defile"] = {
      charName = "Karthus",
      spellSlot = "E",
      range = 550,
      width = 160,
      speed = 1000,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["FallenOne"] = {
      charName = "Karthus",
      spellSlot = "R",
      range = 20000,
      width = 0,
      speed = math.huge,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false,
      timer = 2200
    },
    ["KarmaQ"] = {
      charName = "Karma",
      spellSlot = "Q",
      range = 950,
      width = 90,
      speed = 902,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["KarmaSpiritBind"] = {
      charName = "Karma",
      spellSlot = "W",
      range = 700,
      width = 60,
      speed = 2000,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["KarmaSolKimShield"] = {
      charName = "Karma",
      spellSlot = "E",
      range = 800,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      shieldSlot = _E
    },
    ["KarmaMantra"] = {
      charName = "Karma",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = 1300,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["NullLance"] = {
      charName = "Kassadin",
      spellSlot = "Q",
      range = 650,
      width = 0,
      speed = 1400,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "extreme",
      cc = trueww,
      hitLineCheck = false
    },
    ["NetherBlade"] = {
      charName = "Kassadin",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["ForcePulse"] = {
      charName = "Kassadin",
      spellSlot = "E",
      range = 700,
      width = 10,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["RiftWalk"] = {
      charName = "Kassadin",
      spellSlot = "R",
      range = 675,
      width = 150,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["KatarinaQ"] = {
      charName = "Katarina",
      spellSlot = "Q",
      range = 675,
      width = 0,
      speed = 1800,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["KatarinaW"] = {
      charName = "Katarina",
      spellSlot = "W",
      range = 400,
      width = 0,
      speed = 1800,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["KatarinaE"] = {
      charName = "Katarina",
      spellSlot = "E",
      range = 700,
      width = 0,
      speed = 0,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["KatarinaR"] = {
      charName = "Katarina",
      spellSlot = "R",
      range = 550,
      width = 0,
      speed = 1450,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["JudicatorReckoning"] = {
      charName = "Kayle",
      spellSlot = "Q",
      range = 650,
      width = 0,
      speed = 1500,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["JudicatorDevineBlessing"] = {
      charName = "Kayle",
      spellSlot = "W",
      range = 900,
      width = 0,
      speed = math.huge,
      delay = 0.22,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      healSlot = _W
    },
    ["JudicatorRighteousFury"] = {
      charName = "Kayle",
      spellSlot = "E",
      range = 0,
      width = 0,
      speed = 779,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["JudicatorIntervention"] = {
      charName = "Kayle",
      spellSlot = "R",
      range = 900,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      ultSlot = _R
    },
    ["KennenShurikenHurlMissile1"] = {
      charName = "Kennen",
      spellSlot = "Q",
      range = 1000,
      width = 0,
      speed = 1700,
      delay = 0.69,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["KennenBringTheLight"] = {
      charName = "Kennen",
      spellSlot = "W",
      range = 900,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["KennenLightningRush"] = {
      charName = "Kennen",
      spellSlot = "E",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["KennenShurikenStorm "] = {
      charName = "Kennen",
      spellSlot = "R",
      range = 550,
      width = 0,
      speed = 779,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["KhazixQ"] = {
      charName = "Khazix",
      spellSlot = "Q",
      range = 325,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["KhazixW"] = {
      charName = "Khazix",
      spellSlot = "W",
      range = 1000,
      width = 60,
      speed = 828.5,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["KhazixE"] = {
      charName = "Khazix",
      spellSlot = "E",
      range = 600,
      width = 300,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["KhazixR"] = {
      charName = "Khazix",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["khazixqlong"] = {
      charName = "Khazix",
      spellSlot = "Q",
      range = 375,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["khazixwlong"] = {
      charName = "Khazix",
      spellSlot = "W",
      range = 1000,
      width = 250,
      speed = 828.5,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["khazixelong"] = {
      charName = "Khazix",
      spellSlot = "E",
      range = 900,
      width = 300,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["khazixrlong"] = {
      charName = "Khazix",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["KogMawCausticSpittle"] = {
      charName = "KogMaw",
      spellSlot = "Q",
      range = 625,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["KogMawBioArcanBarrage"] = {
      charName = "KogMaw",
      spellSlot = "W",
      range = 130,
      width = 0,
      speed = 2000,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["KogMawVoidOoze"] = {
      charName = "KogMaw",
      spellSlot = "E",
      range = 1000,
      width = 120,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["KogMawLivingArtillery"] = {
      charName = "KogMaw",
      spellSlot = "R",
      range = 1400,
      width = 225,
      speed = 2000,
      delay = 0.6,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["LeblancChaosOrb"] = {
      charName = "Leblanc",
      spellSlot = "Q",
      range = 700,
      width = 0,
      speed = 2000,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["LeblancSlide"] = {
      charName = "Leblanc",
      spellSlot = "W",
      range = 600,
      width = 220,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["leblacslidereturn"] = {
      charName = "Leblanc",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["LeblancSoulShackle"] = {
      charName = "Leblanc",
      spellSlot = "E",
      range = 925,
      width = 70,
      speed = 1600,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["LeblancChaosOrbM"] = {
      charName = "Leblanc",
      spellSlot = "R",
      range = 700,
      width = 0,
      speed = 2000,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["LeblancSlideM"] = {
      charName = "Leblanc",
      spellSlot = "R",
      range = 600,
      width = 220,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["leblancslidereturnm"] = {
      charName = "Leblanc",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["LeblancSoulShackleM"] = {
      charName = "Leblanc",
      spellSlot = "R",
      range = 925,
      width = 70,
      speed = 1600,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["BlindMonkQOne"] = {
      charName = "LeeSin",
      spellSlot = "Q",
      range = 1000,
      width = 60,
      speed = 1800,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["BlindMonkWOne"] = {
      charName = "LeeSin",
      spellSlot = "W",
      range = 700,
      width = 0,
      speed = 1500,
      delay = 0,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      shieldSlot = _W
    },
    ["BlindMonkEOne"] = {
      charName = "LeeSin",
      spellSlot = "E",
      range = 425,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["BlindMonkRKick"] = {
      charName = "LeeSin",
      spellSlot = "R",
      range = 375,
      width = 0,
      speed = 1500,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["blindmonkqtwo"] = {
      charName = "LeeSin",
      spellSlot = "Q",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["blindmonkwtwo"] = {
      charName = "LeeSin",
      spellSlot = "W",
      range = 700,
      width = 0,
      speed = math.huge,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["blindmonketwo"] = {
      charName = "LeeSin",
      spellSlot = "E",
      range = 425,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["LeonaShieldOfDaybreak"] = {
      charName = "Leona",
      spellSlot = "Q",
      range = 215,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = true,
      hitLineCheck = false
    },
    ["LeonaSolarBarrier"] = {
      charName = "Leona",
      spellSlot = "W",
      range = 500,
      width = 0,
      speed = 0,
      delay = 3,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = true,
      hitLineCheck = false
    },
    ["LeonaZenithBlade"] = {
      charName = "Leona",
      spellSlot = "E",
      range = 900,
      width = 85,
      speed = 2000,
      delay = 0,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["LeonaZenithBladeMissle"] = {
      charName = "Leona",
      spellSlot = "E",
      range = 900,
      width = 85,
      speed = 2000,
      delay = 0,
      SpellType = "skillshot",
      collision = true,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["LeonaSolarFlare"] = {
      charName = "Leona",
      spellSlot = "R",
      range = 1200,
      width = 315,
      speed = math.huge,
      delay = 0.7,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["LissandraQ"] = {
      charName = "Lissandra",
      spellSlot = "Q",
      range = 725,
      width = 75,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = truew
    },
    ["LissandraW"] = {
      charName = "Lissandra",
      spellSlot = "W",
      range = 450,
      width = 80,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["LissandraE"] = {
      charName = "Lissandra",
      spellSlot = "E",
      range = 1050,
      width = 110,
      speed = 850,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["LissandraR"] = {
      charName = "Lissandra",
      spellSlot = "R",
      range = 550,
      width = 0,
      speed = math.huge,
      delay = 0,
      SpellType = "selfEnemyCast",
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true,
      timer = 0,
      zhonyaSlot = _R
    },
    ["LucianQ"] = {
      charName = "Lucian",
      spellSlot = "Q",
      range = 550,
      width = 65,
      speed = 500,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["LucianW"] = {
      charName = "Lucian",
      spellSlot = "W",
      range = 1000,
      width = 80,
      speed = 500,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["LucianE"] = {
      charName = "Lucian",
      spellSlot = "E",
      range = 650,
      width = 50,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["LucianR"] = {
      charName = "Lucian",
      spellSlot = "R",
      range = 1400,
      width = 60,
      speed = math.huge,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["LuluQ"] = {
      charName = "Lulu",
      spellSlot = "Q",
      range = 925,
      width = 80,
      speed = 1400,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["LuluW"] = {
      charName = "Lulu",
      spellSlot = "W",
      range = 650,
      width = 0,
      speed = 2000,
      delay = 0.64,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["LuluE"] = {
      charName = "Lulu",
      spellSlot = "E",
      range = 650,
      width = 0,
      speed = math.huge,
      delay = 0.64,
      SpellType = "everyCast",
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false,
      shieldSlot = _E
    },
    ["LuluR"] = {
      charName = "Lulu",
      spellSlot = "R",
      range = 900,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false,
      ultSlot = _R
    },
    ["LuxLightBinding"] = {
      charName = "Lux",
      spellSlot = "Q",
      range = 1300,
      width = 80,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["LuxPrismaticWave"] = {
      charName = "Lux",
      spellSlot = "W",
      range = 1075,
      width = 150,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      shieldSlot = _E
    },
    ["LuxLightStrikeKugel"] = {
      charName = "Lux",
      spellSlot = "E",
      range = 1100,
      width = 275,
      speed = 1300,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["luxlightstriketoggle"] = {
      charName = "Lux",
      spellSlot = "E",
      range = 1100,
      width = 275,
      speed = 1300,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["LuxMaliceCannon"] = {
      charName = "Lux",
      spellSlot = "R",
      range = 3340,
      width = 190,
      speed = 3000,
      delay = 1.75,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["SeismicShard"] = {
      charName = "Malphite",
      spellSlot = "Q",
      range = 625,
      width = 0,
      speed = 1200,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["Obduracy"] = {
      charName = "Malphite",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["Landslide"] = {
      charName = "Malphite",
      spellSlot = "E",
      range = 400,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["UFSlash"] = {
      charName = "Malphite",
      spellSlot = "R",
      range = 1000,
      width = 270,
      speed = 700,
      delay = 0,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["AlZaharCalloftheVoid"] = {
      charName = "Malzahar",
      spellSlot = "Q",
      range = 900,
      width = 110,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["AlZaharNullZone"] = {
      charName = "Malzahar",
      spellSlot = "W",
      range = 800,
      width = 250,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["AlZaharMaleficVisions"] = {
      charName = "Malzahar",
      spellSlot = "E",
      range = 650,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["AlZaharNetherGrasp"] = {
      charName = "Malzahar",
      spellSlot = "R",
      range = 700,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["MaokaiTrunkLine"] = {
      charName = "Maokai",
      spellSlot = "Q",
      range = 600,
      width = 110,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["MaokaiUnstableGrowth"] = {
      charName = "Maokai",
      spellSlot = "W",
      range = 650,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["MaokaiSapling2"] = {
      charName = "Maokai",
      spellSlot = "E",
      range = 1100,
      width = 250,
      speed = 1750,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["MaokaiDrain3"] = {
      charName = "Maokai",
      spellSlot = "R",
      range = 625,
      width = 575,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillShoot",
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["AlphaStrike"] = {
      charName = "MasterYi",
      spellSlot = "Q",
      range = 600,
      width = 0,
      speed = 4000,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["Meditate"] = {
      charName = "MasterYi",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["WujuStyle"] = {
      charName = "MasterYi",
      spellSlot = "E",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.23,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["Highlander"] = {
      charName = "MasterYi",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.37,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      slowSlot = _R
    },
    ["MissFortuneRicochetShot"] = {
      charName = "MissFortune",
      spellSlot = "Q",
      range = 650,
      width = 0,
      speed = 1400,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["MissFortuneViciousStrikes"] = {
      charName = "MissFortune",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["MissFortuneScattershot"] = {
      charName = "MissFortune",
      spellSlot = "E",
      range = 1000,
      width = 400,
      speed = 500,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["MissFortuneBulletTime"] = {
      charName = "MissFortune",
      spellSlot = "R",
      range = 1400,
      width = 100,
      speed = 775,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = false,
      hitLineCheck = true
    },
    ["MordekaiserMaceOfSpades"] = {
      charName = "Mordekaiser",
      spellSlot = "Q",
      range = 600,
      width = 0,
      speed = 1500,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["MordekaiserCreepinDeathCast"] = {
      charName = "Mordekaiser",
      spellSlot = "W",
      range = 750,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["MordekaiserSyphoneOfDestruction"] = {
      charName = "Mordekaiser",
      spellSlot = "E",
      range = 700,
      width = 0,
      speed = 1500,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["MordekaiserChildrenOfTheGrave"] = {
      charName = "Mordekaiser",
      spellSlot = "R",
      range = 850,
      width = 0,
      speed = 1500,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["DarkBindingMissile"] = {
      charName = "Morgana",
      spellSlot = "Q",
      range = 1175,
      width = 70,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["TormentedSoil"] = {
      charName = "Morgana",
      spellSlot = "W",
      range = 1075,
      width = 350,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["BlackShield"] = {
      charName = "Morgana",
      spellSlot = "E",
      range = 750,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      shieldSlot = _E
    },
    ["SoulShackles"] = {
      charName = "Morgana",
      spellSlot = "R",
      range = 1,
      width = 1000,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true,
      timer = 2800
    },
    ["NamiQ"] = {
      charName = "Nami",
      spellSlot = "Q",
      range = 875,
      width = 200,
      speed = 1750,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["NamiW"] = {
      charName = "Nami",
      spellSlot = "W",
      range = 725,
      width = 0,
      speed = 1100,
      delay = 0.5,
      SpellType = "everyCast",
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false,
      healSlot = _W
    },
    ["NamiE"] = {
      charName = "Nami",
      spellSlot = "E",
      range = 800,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["NamiR"] = {
      charName = "Nami",
      spellSlot = "R",
      range = 2550,
      width = 600,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["NasusQ"] = {
      charName = "Nasus",
      spellSlot = "Q",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["NasusW"] = {
      charName = "Nasus",
      spellSlot = "W",
      range = 600,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = true,
      hitLineCheck = false
    },
    ["NasusE"] = {
      charName = "Nasus",
      spellSlot = "E",
      range = 850,
      width = 400,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["NasusR"] = {
      charName = "Nasus",
      spellSlot = "R",
      range = 1,
      width = 350,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["NautilusAnchorDrag"] = {
      charName = "Nautilus",
      spellSlot = "Q",
      range = 950,
      width = 80,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = truew
    },
    ["NautilusPiercingGaze"] = {
      charName = "Nautilus",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      shieldSlot = _W
    },
    ["NautilusSplashZone"] = {
      charName = "Nautilus",
      spellSlot = "E",
      range = 600,
      width = 60,
      speed = 1300,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["NautilusGandLine"] = {
      charName = "Nautilus",
      spellSlot = "R",
      range = 1500,
      width = 60,
      speed = 1400,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false,
      timer = 450 - GetLatency()
    },
    ["JavelinToss"] = {
      charName = "Nidalee",
      spellSlot = "Q",
      range = 1500,
      width = 60,
      speed = 1300,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["Bushwhack"] = {
      charName = "Nidalee",
      spellSlot = "W",
      range = 900,
      width = 125,
      speed = 1450,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["PrimalSurge"] = {
      charName = "Nidalee",
      spellSlot = "E",
      range = 600,
      width = 0,
      speed = math.huge,
      delay = 0,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      healSlot = _E
    },
    ["AspectOfTheCougar"] = {
      charName = "Nidalee",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["Takedown"] = {
      charName = "Nidalee",
      spellSlot = "Q",
      range = 50,
      width = 0,
      speed = 500,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["Pounce"] = {
      charName = "Nidalee",
      spellSlot = "W",
      range = 375,
      width = 150,
      speed = 1500,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["Swipe"] = {
      charName = "Nidalee",
      spellSlot = "E",
      range = 300,
      width = 300,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["NocturneDuskbringer"] = {
      charName = "Nocturne",
      spellSlot = "Q",
      range = 1125,
      width = 60,
      speed = 1600,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["NocturneShroudofDarkness"] = {
      charName = "Nocturne",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = 500,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      shieldSlot = _E
    },
    ["NocturneUnspeakableHorror"] = {
      charName = "Nocturne",
      spellSlot = "E",
      range = 500,
      width = 0,
      speed = 0,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = true,
      hitLineCheck = false
    },
    ["NocturneParanoia"] = {
      charName = "Nocturne",
      spellSlot = "R",
      range = 2000,
      width = 0,
      speed = 500,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["Consume"] = {
      charName = "Nunu",
      spellSlot = "Q",
      range = 125,
      width = 60,
      speed = 1400,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = false,
      hitLineCheck = false
    },
    ["BloodBoil"] = {
      charName = "Nunu",
      spellSlot = "W",
      range = 700,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["IceBlast"] = {
      charName = "Nunu",
      spellSlot = "E",
      range = 550,
      width = 0,
      speed = 1000,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["AbsoluteZero"] = {
      charName = "Nunu",
      spellSlot = "R",
      range = 1,
      width = 650,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfcast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["OlafAxeThrowCast"] = {
      charName = "Olaf",
      spellSlot = "Q",
      range = 1000,
      width = 90,
      speed = 1600,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["OlafFrenziedStrikes"] = {
      charName = "Olaf",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["OlafRecklessStrike"] = {
      charName = "Olaf",
      spellSlot = "E",
      range = 325,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["OlafRagnarok"] = {
      charName = "Olaf",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      qssSlot = _R
    },
    ["OrianaIzunaCommand"] = {
      charName = "Orianna",
      spellSlot = "Q",
      range = 825,
      width = 145,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["OrianaDissonanceCommand"] = {
      charName = "Orianna",
      spellSlot = "W",
      range = 0,
      width = 260,
      speed = 1200,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["OrianaRedactCommand"] = {
      charName = "Orianna",
      spellSlot = "E",
      range = 1095,
      width = 145,
      speed = 1200,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false,
      shieldSlot = _E
    },
    ["OrianaDetonateCommand"] = {
      charName = "Orianna",
      spellSlot = "R",
      range = 0,
      width = 425,
      speed = 1200,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["Pantheon_Throw"] = {
      charName = "Pantheon",
      spellSlot = "Q",
      range = 600,
      width = 0,
      speed = 1500,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["Pantheon_LeapBash"] = {
      charName = "Pantheon",
      spellSlot = "W",
      range = 600,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["Pantheon_Heartseeker"] = {
      charName = "Pantheon",
      spellSlot = "E",
      range = 600,
      width = 100,
      speed = 775,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["Pantheon_GrandSkyfall_Jump"] = {
      charName = "Pantheon",
      spellSlot = "R",
      range = 5500,
      width = 1000,
      speed = 3000,
      delay = 1,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["PoppyDevastatingBlow"] = {
      charName = "Poppy",
      spellSlot = "Q",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["PoppyParagonOfDemacia"] = {
      charName = "Poppy",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["PoppyHeroicCharge"] = {
      charName = "Poppy",
      spellSlot = "E",
      range = 525,
      width = 0,
      speed = 1450,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["PoppyDiplomaticImmunity"] = {
      charName = "Poppy",
      spellSlot = "R",
      range = 900,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["QuinnQ"] = {
      charName = "Quinn",
      spellSlot = "Q",
      range = 1025,
      width = 80,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["QuinnW"] = {
      charName = "Quinn",
      spellSlot = "W",
      range = 2100,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = falsee
    },
    ["QuinnE"] = {
      charName = "Quinn",
      spellSlot = "E",
      range = 700,
      width = 0,
      speed = 775,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["QuinnR"] = {
      charName = "Quinn",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["PowerBall"] = {
      charName = "Rammus",
      spellSlot = "Q",
      range = 1,
      width = 200,
      speed = 775,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["DefensiveBallCurl"] = {
      charName = "Rammus",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["PuncturingTaunt"] = {
      charName = "Rammus",
      spellSlot = "E",
      range = 325,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["Tremors2"] = {
      charName = "Rammus",
      spellSlot = "R",
      range = 1,
      width = 300,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["RenektonCleave"] = {
      charName = "Renekton",
      spellSlot = "Q",
      range = 1,
      width = 450,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["RenektonPreExecute"] = {
      charName = "Renekton",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["RenektonSliceAndDice"] = {
      charName = "Renekton",
      spellSlot = "E",
      range = 450,
      width = 50,
      speed = 1400,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["RenektonReignOfTheTyrant"] = {
      charName = "Renekton",
      spellSlot = "R",
      range = 1,
      width = 530,
      speed = 775,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["RengarQ"] = {
      charName = "Rengar",
      spellSlot = "Q",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["RengarW"] = {
      charName = "Rengar",
      spellSlot = "W",
      range = 1,
      width = 500,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["RengarE"] = {
      charName = "Rengar",
      spellSlot = "E",
      range = 575,
      width = 0,
      speed = 1800,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["RengarR"] = {
      charName = "Rengar",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["RivenTriCleav"] = {
      charName = "Riven",
      spellSlot = "Q",
      range = 250,
      width = 0,
      speed = 0,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["RivenTriCleave_03"] = {
      charName = "Riven",
      spellSlot = "Q",
      range = 250,
      width = 0,
      speed = 0,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["RivenMartyr"] = {
      charName = "Riven",
      spellSlot = "W",
      range = 260,
      width = 0,
      speed = 1500,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["RivenFeint"] = {
      charName = "Riven",
      spellSlot = "E",
      range = 325,
      width = 0,
      speed = 1450,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      shieldSlot = _E
    },
    ["RivenFengShuiEngine"] = {
      charName = "Riven",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = 1200,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["rivenizunablade"] = {
      charName = "Riven",
      spellSlot = "R",
      range = 900,
      width = 200,
      speed = 1450,
      delay = 0.3,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["RumbleFlameThrower"] = {
      charName = "Rumble",
      spellSlot = "Q",
      range = 600,
      width = 10,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["RumbleShield"] = {
      charName = "Rumble",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      shieldSlot = _W
    },
    ["RumbeGrenade"] = {
      charName = "Rumble",
      spellSlot = "E",
      range = 850,
      width = 90,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["RumbleCarpetBomb"] = {
      charName = "Rumble",
      spellSlot = "R",
      range = 625,
      width = 0,
      speed = 1400,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["Overload"] = {
      charName = "Ryze",
      spellSlot = "Q",
      range = 625,
      width = 0,
      speed = 1400,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["RunePrison"] = {
      charName = "Ryze",
      spellSlot = "W",
      range = 600,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["SpellFlux"] = {
      charName = "Ryze",
      spellSlot = "E",
      range = 600,
      width = 0,
      speed = 1000,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["DesperatePower"] = {
      charName = "Ryze",
      spellSlot = "R",
      range = 625,
      width = 0,
      speed = 1400,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["SejuaniArcticAssault"] = {
      charName = "Sejuani",
      spellSlot = "Q",
      range = 650,
      width = 75,
      speed = 1450,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["SejuaniNorthernWinds"] = {
      charName = "Sejuani",
      spellSlot = "W",
      range = 1,
      width = 350,
      speed = 1500,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["SejuaniWintersClaw"] = {
      charName = "Sejuani",
      spellSlot = "E",
      range = 1,
      width = 1000,
      speed = 1450,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["SejuaniGlacialPrisonStart"] = {
      charName = "Sejuani",
      spellSlot = "R",
      range = 1175,
      width = 110,
      speed = 1400,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["Deceive"] = {
      charName = "Shaco",
      spellSlot = "Q",
      range = 400,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["JackInTheBox"] = {
      charName = "Shaco",
      spellSlot = "W",
      range = 425,
      width = 60,
      speed = 1450,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["TwoShivPoisen"] = {
      charName = "Shaco",
      spellSlot = "E",
      range = 625,
      width = 0,
      speed = 1500,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["HallucinateFull"] = {
      charName = "Shaco",
      spellSlot = "R",
      range = 1125,
      width = 250,
      speed = 395,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["ShenVorpalStar"] = {
      charName = "Shen",
      spellSlot = "Q",
      range = 475,
      width = 0,
      speed = 1500,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["ShenFeint"] = {
      charName = "Shen",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      shieldSlot = _W
    },
    ["ShenShadowDash"] = {
      charName = "Shen",
      spellSlot = "E",
      range = 600,
      width = 50,
      speed = 1000,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["ShenStandUnited"] = {
      charName = "Shen",
      spellSlot = "R",
      range = 75000,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      ultSlot = _R
    },
    ["ShyvanaDoubleAttack"] = {
      charName = "Shyvana",
      spellSlot = "Q",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["ShyvanaImmolationAura"] = {
      charName = "Shyvana",
      spellSlot = "W",
      range = 1,
      width = 325,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["ShyvanaFireball"] = {
      charName = "Shyvana",
      spellSlot = "E",
      range = 925,
      width = 60,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = true
    },
    ["ShyvanaTransformCast"] = {
      charName = "Shyvana",
      spellSlot = "R",
      range = 1000,
      width = 160,
      speed = 700,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = true
    },
    ["PoisenTrail"] = {
      charName = "Singed",
      spellSlot = "Q",
      range = 0,
      width = 400,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["MegaAdhesive"] = {
      charName = "Singed",
      spellSlot = "W",
      range = 1175,
      width = 350,
      speed = 700,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["Fling"] = {
      charName = "Singed",
      spellSlot = "E",
      range = 125,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["InsanityPotion"] = {
      charName = "Singed",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["CrypticGaze"] = {
      charName = "Sion",
      spellSlot = "Q",
      range = 550,
      width = 0,
      speed = 1600,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["DeathsCaressFull"] = {
      charName = "Sion",
      spellSlot = "W",
      range = 550,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false,
      shieldSlot = _W
    },
    ["Enrage"] = {
      charName = "Sion",
      spellSlot = "E",
      range = 1,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["Cannibalism"] = {
      charName = "Sion",
      spellSlot = "R",
      range = 1,
      width = 0,
      speed = 500,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["SivirQ"] = {
      charName = "Sivir",
      spellSlot = "Q",
      range = 1075,
      width = 90,
      speed = 1350,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["SivirW"] = {
      charName = "Sivir",
      spellSlot = "W",
      range = 500,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["SivirE"] = {
      charName = "Sivir",
      spellSlot = "E",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      shieldSlot = _E
    },
    ["SivirR"] = {
      charName = "Sivir",
      spellSlot = "R",
      range = 0,
      width = 1000,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["SkarnerVirulentSlash"] = {
      charName = "Skarner",
      spellSlot = "Q",
      range = 350,
      width = 0,
      speed = math.huge,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["SkarnerExoskeleton"] = {
      charName = "Skarner",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      shieldSlot = _W
    },
    ["SkarnerFracture"] = {
      charName = "Skarner",
      spellSlot = "E",
      range = 1000,
      width = 60,
      speed = 1200,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["SkarnerImpale"] = {
      charName = "Skarner",
      spellSlot = "R",
      range = 350,
      width = 0,
      speed = math.huge,
      delay = 0,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["SonaHymnofValor"] = {
      charName = "Sona",
      spellSlot = "Q",
      range = 700,
      width = 0,
      speed = 1500,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["SonaAriaofPerseverance"] = {
      charName = "Sona",
      spellSlot = "W",
      range = 1000,
      width = 0,
      speed = 1500,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      healSlot = _W
    },
    ["SonaSongofDiscord"] = {
      charName = "Sona",
      spellSlot = "E",
      range = 1000,
      width = 0,
      speed = 1500,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["SonaCrescendo"] = {
      charName = "Sona",
      spellSlot = "R",
      range = 900,
      width = 600,
      speed = 2400,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false,
      timer = 0
    },
    ["Starcall"] = {
      charName = "Soraka",
      spellSlot = "Q",
      range = 675,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfcast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["AstralBlessing"] = {
      charName = "Soraka",
      spellSlot = "W",
      range = 750,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      healSlot = _W
    },
    ["InfuseWrapper"] = {
      charName = "Soraka",
      spellSlot = "E",
      range = 725,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "everyCast",
      riskLevel = "dangerous",
      cc = false,
      hitLineCheck = false
    },
    ["Wish"] = {
      charName = "Soraka",
      spellSlot = "R",
      range = 75000,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      ultSlot = _R
    },
    ["SwainDecrepify"] = {
      charName = "Swain",
      spellSlot = "Q",
      range = 625,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = frue,
      hitLineCheck = false
    },
    ["SwainShadowGrasp"] = {
      charName = "Swain",
      spellSlot = "W",
      range = 1040,
      width = 275,
      speed = 1250,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = frue,
      hitLineCheck = false
    },
    ["SwainTorment"] = {
      charName = "Swain",
      spellSlot = "E",
      range = 625,
      width = 0,
      speed = 1400,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["SwainMetamorphism"] = {
      charName = "Swain",
      spellSlot = "R",
      range = 0,
      width = 700,
      speed = 950,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["SyndraQ"] = {
      charName = "Syndra",
      spellSlot = "Q",
      range = 800,
      width = 180,
      speed = 1750,
      delay = 0.25,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["SyndraW "] = {
      charName = "Syndra",
      spellSlot = "W",
      range = 600,
      width = 0,
      speed = 1450,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["SyndraE"] = {
      charName = "Syndra",
      spellSlot = "E",
      range = 100,
      width = 0,
      speed = 902,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["SyndraR"] = {
      charName = "Syndra",
      spellSlot = "R",
      range = 1010,
      width = 0,
      speed = 1100,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["TalonNoxianDiplomacy"] = {
      charName = "Talon",
      spellSlot = "Q",
      range = 0,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["TalonRake"] = {
      charName = "Talon",
      spellSlot = "W",
      range = 750,
      width = 0,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["TalonCutthroat"] = {
      charName = "Talon",
      spellSlot = "E",
      range = 750,
      width = 0,
      speed = 1200,
      delay = 0,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["TalonShadowAssault"] = {
      charName = "Talon",
      spellSlot = "R",
      range = 750,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["Imbue"] = {
      charName = "Taric",
      spellSlot = "Q",
      range = 750,
      width = 0,
      speed = 1200,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      healSlot = _Q
    },
    ["Shatter"] = {
      charName = "Taric",
      spellSlot = "W",
      range = 400,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["Dazzle"] = {
      charName = "Taric",
      spellSlot = "E",
      range = 625,
      width = 0,
      speed = 1400,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["TaricHammerSmash"] = {
      charName = "Taric",
      spellSlot = "R",
      range = 400,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["BlindingDart"] = {
      charName = "Teemo",
      spellSlot = "Q",
      range = 580,
      width = 0,
      speed = 1500,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["MoveQuick"] = {
      charName = "Teemo",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = 943,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["ToxicShot"] = {
      charName = "Teemo",
      spellSlot = "E",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["BantamTrap"] = {
      charName = "Teemo",
      spellSlot = "R",
      range = 230,
      width = 0,
      speed = 1500,
      delay = 0,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = true,
      hitLineCheck = false
    },
    ["ThreshQ"] = {
      charName = "Thresh",
      spellSlot = "Q",
      range = 1075,
      width = 60,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["ThreshW"] = {
      charName = "Thresh",
      spellSlot = "W",
      range = 950,
      width = 315,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      shieldSlot = _W
    },
    ["ThreshE"] = {
      charName = "Thresh",
      spellSlot = "E",
      range = 515,
      width = 160,
      speed = math.huge,
      delay = 0.3,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["ThreshRPenta"] = {
      charName = "Thresh",
      spellSlot = "R",
      range = 420,
      width = 420,
      speed = math.huge,
      delay = 0.3,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["RapidFire"] = {
      charName = "Tristana",
      spellSlot = "Q",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["RocketJump"] = {
      charName = "Tristana",
      spellSlot = "W",
      range = 900,
      width = 270,
      speed = 1150,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["DetonatingShot"] = {
      charName = "Tristana",
      spellSlot = "E",
      range = 625,
      width = 0,
      speed = 1400,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["BusterShot"] = {
      charName = "Tristana",
      spellSlot = "R",
      range = 700,
      width = 0,
      speed = 1600,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["TrundleTrollSmash"] = {
      charName = "Trundle",
      spellSlot = "Q",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["trundledesecrate"] = {
      charName = "Trundle",
      spellSlot = "W",
      range = 0,
      width = 900,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "noDmg",
      cc = true,
      hitLineCheck = false
    },
    ["TrundleCircle"] = {
      charName = "Trundle",
      spellSlot = "E",
      range = 1100,
      width = 188,
      speed = 1600,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["TrundlePain"] = {
      charName = "Trundle",
      spellSlot = "R",
      range = 700,
      width = 0,
      speed = 1400,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["Bloodlust"] = {
      charName = "Tryndamere",
      spellSlot = "Q",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["MockingShout"] = {
      charName = "Tryndamere",
      spellSlot = "W",
      range = 400,
      width = 400,
      speed = 500,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["slashCast"] = {
      charName = "Tryndamere",
      spellSlot = "E",
      range = 660,
      width = 225,
      speed = 700,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["UndyingRage"] = {
      charName = "Tryndamere",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      ultSlot = _R
    },
    ["WildCards"] = {
      charName = "TwistedFate",
      spellSlot = "Q",
      range = 1450,
      width = 80,
      speed = 1450,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["PickACard"] = {
      charName = "TwistedFate",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["CardmasterStack"] = {
      charName = "TwistedFate",
      spellSlot = "E",
      range = 525,
      width = 0,
      speed = 1200,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["Destiny"] = {
      charName = "TwistedFate",
      spellSlot = "R",
      range = 5500,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["HideInShadows"] = {
      charName = "Twitch",
      spellSlot = "Q",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["TwitchVenomCask"] = {
      charName = "Twitch",
      spellSlot = "W",
      range = 800,
      width = 275,
      speed = 1750,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["TwitchVenomCaskMissle"] = {
      charName = "Twitch",
      spellSlot = "W",
      range = 800,
      width = 275,
      speed = 1750,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["Expunge"] = {
      charName = "Twitch",
      spellSlot = "E",
      range = 1200,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["FullAutomatic"] = {
      charName = "Twitch",
      spellSlot = "R",
      range = 850,
      width = 0,
      speed = 500,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["UdyrTigerStance"] = {
      charName = "Udyr",
      spellSlot = "Q",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["UdyrTurtleStance"] = {
      charName = "Udyr",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      shieldSlot = _W
    },
    ["UdyrBearStance"] = {
      charName = "Udyr",
      spellSlot = "E",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = true,
      hitLineCheck = false
    },
    ["UdyrPhoenixStance"] = {
      charName = "Udyr",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["UrgotHeatseekingMissile"] = {
      charName = "Urgot",
      spellSlot = "Q",
      range = 1000,
      width = 80,
      speed = 1600,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["UrgotTerrorCapacitorActive2"] = {
      charName = "Urgot",
      spellSlot = "W",
      range = 0,
      width = 300,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false,
      shieldSlot = _W
    },
    ["UrgotPlasmaGrenade"] = {
      charName = "Urgot",
      spellSlot = "E",
      range = 950,
      width = 0,
      speed = 1750,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["UrgotSwap2"] = {
      charName = "Urgot",
      spellSlot = "R",
      range = 850,
      width = 0,
      speed = 1800,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["VarusQ"] = {
      charName = "Varus",
      spellSlot = "Q",
      range = 1500,
      width = 100,
      speed = 1500,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["VarusW"] = {
      charName = "Varus",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = 0,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["VarusE"] = {
      charName = "Varus",
      spellSlot = "E",
      range = 800,
      width = 55,
      speed = 1500,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["VarusR"] = {
      charName = "Varus",
      spellSlot = "R",
      range = 800,
      width = 100,
      speed = 1500,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["VayneTumble"] = {
      charName = "Vayne",
      spellSlot = "Q",
      range = 250,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["VayneSilverBolts"] = {
      charName = "Vayne",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["VayneCondemm"] = {
      charName = "Vayne",
      spellSlot = "E",
      range = 450,
      width = 0,
      speed = 1200,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["vayneinquisition"] = {
      charName = "Vayne",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["VeigarBalefulStrike"] = {
      charName = "Veigar",
      spellSlot = "Q",
      range = 650,
      width = 0,
      speed = 1500,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["VeigarDarkMatter"] = {
      charName = "Veigar",
      spellSlot = "W",
      range = 900,
      width = 225,
      speed = 1500,
      delay = 1.2,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["VeigarEventHorizon"] = {
      charName = "Veigar",
      spellSlot = "E",
      range = 813,
      width = 425,
      speed = 1500,
      delay = math.huge,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["VeigarPrimordialBurst"] = {
      charName = "Veigar",
      spellSlot = "R",
      range = 650,
      width = 0,
      speed = 1400,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false,
      timer = 230 - GetLatency()
    },
    ["VelkozQ"] = {
      charName = "Velkoz",
      spellSlot = "Q",
      range = 1050,
      width = 60,
      speed = 1200,
      delay = 0.3,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["VelkozQMissle"] = {
      charName = "Velkoz",
      spellSlot = "Q",
      range = 1050,
      width = 60,
      speed = 1200,
      delay = 0,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["velkozqplitactive"] = {
      charName = "Velkoz",
      spellSlot = "Q",
      range = 1050,
      width = 60,
      speed = 1200,
      delay = 0.8,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["VelkozW"] = {
      charName = "Velkoz",
      spellSlot = "W",
      range = 1050,
      width = 90,
      speed = 1200,
      delay = 0,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["VelkozE"] = {
      charName = "Velkoz",
      spellSlot = "E",
      range = 850,
      width = 0,
      speed = 500,
      delay = 0,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["VelkozR"] = {
      charName = "Velkoz",
      spellSlot = "R",
      range = 1575,
      width = 0,
      speed = 1500,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["ViQ"] = {
      charName = "Vi",
      spellSlot = "Q",
      range = 600,
      width = 55,
      speed = 1500,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["ViW"] = {
      charName = "Vi",
      spellSlot = "W",
      range = 600,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = true,
      hitLineCheck = false
    },
    ["ViE"] = {
      charName = "Vi",
      spellSlot = "E",
      range = 600,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = true,
      hitLineCheck = false
    },
    ["ViR"] = {
      charName = "Vi",
      spellSlot = "R",
      range = 600,
      width = 0,
      speed = 0,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false,
      timer = 230 - GetLatency()
    },
    ["ViktorPowerTransfer"] = {
      charName = "Viktor",
      spellSlot = "Q",
      range = 600,
      width = 0,
      speed = 1400,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["ViktorGravitonField"] = {
      charName = "Viktor",
      spellSlot = "W",
      range = 815,
      width = 300,
      speed = 1750,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["ViktorDeathRa"] = {
      charName = "Viktor",
      spellSlot = "E",
      range = 700,
      width = 90,
      speed = 1210,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["ViktorChaosStorm"] = {
      charName = "Viktor",
      spellSlot = "R",
      range = 700,
      width = 250,
      speed = 1210,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["VladimirTransfusion"] = {
      charName = "Vladimir",
      spellSlot = "Q",
      range = 600,
      width = 0,
      speed = 1400,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["VladimirSanguinePool"] = {
      charName = "Vladimir",
      spellSlot = "W",
      range = 300,
      width = 0,
      speed = 1600,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["VladimirTidesofBlood"] = {
      charName = "Vladimir",
      spellSlot = "E",
      range = 620,
      width = 0,
      speed = 1100,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = true,
      hitLineCheck = false
    },
    ["VladimirHemoplague"] = {
      charName = "Vladimir",
      spellSlot = "R",
      range = 875,
      width = 350,
      speed = 1200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["VolibearQ"] = {
      charName = "Volibear",
      spellSlot = "Q",
      range = 300,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = true,
      hitLineCheck = false
    },
    ["VolibearW"] = {
      charName = "Volibear",
      spellSlot = "W",
      range = 400,
      width = 0,
      speed = 1450,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["VolibearE"] = {
      charName = "Volibear",
      spellSlot = "E",
      range = 425,
      width = 425,
      speed = 825,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = true,
      hitLineCheck = false
    },
    ["VolibearR"] = {
      charName = "Volibear",
      spellSlot = "R",
      range = 425,
      width = 425,
      speed = 825,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["HungeringStrike"] = {
      charName = "Warwick",
      spellSlot = "Q",
      range = 400,
      width = 0,
      speed = math.huge,
      delay = 0,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["HuntersCall"] = {
      charName = "Warwick",
      spellSlot = "W",
      range = 1000,
      width = 0,
      speed = math.huge,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["BlooScent"] = {
      charName = "Warwick",
      spellSlot = "E",
      range = 1500,
      width = 0,
      speed = math.huge,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["InfiniteDuress"] = {
      charName = "Warwick",
      spellSlot = "R",
      range = 700,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["MonkeyKingDoubleAttack"] = {
      charName = "MonkeyKing",
      spellSlot = "Q",
      range = 300,
      width = 0,
      speed = 20,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["MonkeyKingDecoy"] = {
      charName = "MonkeyKing",
      spellSlot = "W",
      range = 0,
      width = 0,
      speed = 0,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["MonkeyKingNimbus"] = {
      charName = "MonkeyKing",
      spellSlot = "E",
      range = 625,
      width = 0,
      speed = 2200,
      delay = 0,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["MonkeyKingSpinToWin"] = {
      charName = "MonkeyKing",
      spellSlot = "R",
      range = 315,
      width = 0,
      speed = 700,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["monkeykingspintowinleave"] = {
      charName = "MonkeyKing",
      spellSlot = "R",
      range = 0,
      width = 0,
      speed = 700,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["XerathArcanoPulseChargeUp"] = {
      charName = "Xerath",
      spellSlot = "Q",
      range = 750,
      width = 100,
      speed = 500,
      delay = 0.75,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["XerathArcaneBarrage2"] = {
      charName = "Xerath",
      spellSlot = "W",
      range = 1100,
      width = 0,
      speed = 20,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["XerathMageSpear"] = {
      charName = "Xerath",
      spellSlot = "E",
      range = 1050,
      width = 70,
      speed = 1600,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["XerathLocusOfPower2"] = {
      charName = "Xerath",
      spellSlot = "R",
      range = 3200,
      width = 0,
      speed = 500,
      delay = 0.75,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["XenZhaoComboTarget"] = {
      charName = "Xin Zhao",
      spellSlot = "Q",
      range = 200,
      width = 0,
      speed = 2000,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = false,
      hitLineCheck = false
    },
    ["XenZhaoBattleCry"] = {
      charName = "Xin Zhao",
      spellSlot = "W",
      range = 20,
      width = 0,
      speed = 2000,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["XenZhaoSweep"] = {
      charName = "Xin Zhao",
      spellSlot = "E",
      range = 600,
      width = 0,
      speed = 1750,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["XenZhaoParry"] = {
      charName = "Xin Zhao",
      spellSlot = "R",
      range = 375,
      width = 0,
      speed = 1750,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["YasuoQW"] = {
      charName = "Yasuo",
      spellSlot = "Q",
      range = 475,
      width = 55,
      speed = 1500,
      delay = 0.75,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["yasuoq2w"] = {
      charName = "Yasuo",
      spellSlot = "Q",
      range = 475,
      width = 55,
      speed = 1500,
      delay = 0.75,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["yasuoq3w"] = {
      charName = "Yasuo",
      spellSlot = "Q",
      range = 1000,
      width = 90,
      speed = 1500,
      delay = 0.75,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["YasuoWMovingWall"] = {
      charName = "Yasuo",
      spellSlot = "W",
      range = 400,
      width = 0,
      speed = 500,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["YasuoDashWrapper"] = {
      charName = "Yasuo",
      spellSlot = "E",
      range = 475,
      width = 0,
      speed = 20,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["YasuoRKnockUpComboW"] = {
      charName = "Yasuo",
      spellSlot = "R",
      range = 1200,
      width = 0,
      speed = 20,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["YorickSpectral"] = {
      charName = "Yorick",
      spellSlot = "Q",
      range = 1,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["YorickDecayed"] = {
      charName = "Yorick",
      spellSlot = "W",
      range = 600,
      width = 200,
      speed = math.huge,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["YorickRavenous"] = {
      charName = "Yorick",
      spellSlot = "E",
      range = 550,
      width = 200,
      speed = math.huge,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = true,
      hitLineCheck = false
    },
    ["YorickReviveAlly"] = {
      charName = "Yorick",
      spellSlot = "R",
      range = 900,
      width = 0,
      speed = 1500,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["ZacQ"] = {
      charName = "Zac",
      spellSlot = "Q",
      range = 550,
      width = 120,
      speed = 902,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = true
    },
    ["ZacW"] = {
      charName = "Zac",
      spellSlot = "W",
      range = 550,
      width = 40,
      speed = 1600,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "extreme",
      cc = false,
      hitLineCheck = false
    },
    ["ZacE"] = {
      charName = "Zac",
      spellSlot = "E",
      range = 300,
      width = 0,
      speed = 1500,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["ZacR"] = {
      charName = "Zac",
      spellSlot = "R",
      range = 850,
      width = 0,
      speed = 1800,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["ZedShuriken"] = {
      charName = "Zed",
      spellSlot = "Q",
      range = 900,
      width = 45,
      speed = 902,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["ZedShdaowDash"] = {
      charName = "Zed",
      spellSlot = "W",
      range = 550,
      width = 40,
      speed = 1600,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["ZedPBAOEDummy"] = {
      charName = "Zed",
      spellSlot = "E",
      range = 300,
      width = 0,
      speed = 0,
      delay = 0,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["zedult"] = {
      charName = "Zed",
      spellSlot = "R",
      range = 850,
      width = 0,
      speed = 0,
      delay = 0.5,
      SpellType = "enemyCast",
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["ZiggsQ"] = {
      charName = "Ziggs",
      spellSlot = "Q",
      range = 850,
      width = 75,
      speed = 1750,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = true,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["ZiggsW"] = {
      charName = "Ziggs",
      spellSlot = "W",
      range = 850,
      width = 0,
      speed = 1750,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false
    },
    ["ZiggsE"] = {
      charName = "Ziggs",
      spellSlot = "E",
      range = 850,
      width = 350,
      speed = 1750,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    },
    ["ZiggsR"] = {
      charName = "Ziggs",
      spellSlot = "R",
      range = 850,
      width = 600,
      speed = 1750,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false,
      timer = 950 - GetLatency()
    },
    ["TimeBomb"] = {
      charName = "Zilean",
      spellSlot = "Q",
      range = 700,
      width = 0,
      speed = 1100,
      delay = 0,
      SpellType = "everyCast",
      riskLevel = "kill",
      cc = false,
      hitLineCheck = false,
      timer = 2100
    },
    ["Rewind"] = {
      charName = "Zilean",
      spellSlot = "W",
      range = 1,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "selfCast",
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["TimeWarp"] = {
      charName = "Zilean",
      spellSlot = "E",
      range = 700,
      width = 0,
      speed = 1100,
      delay = 0.5,
      SpellType = "everyCast",
      riskLevel = "dangerous",
      cc = true,
      hitLineCheck = false
    },
    ["ChronoShift"] = {
      charName = "Zilean",
      spellSlot = "R",
      range = 780,
      width = 0,
      speed = math.huge,
      delay = 0.5,
      SpellType = "allyCast",
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false,
      ultSlot = _R
    },
    ["ZyraQFissure"] = {
      charName = "Zyra",
      spellSlot = "Q",
      range = 800,
      width = 85,
      speed = 1400,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "kill",
      cc = false,
      hitLineCheck = true
    },
    ["ZyraSeed"] = {
      charName = "Zyra",
      spellSlot = "W",
      range = 800,
      width = 0,
      speed = 2200,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "noDmg",
      cc = false,
      hitLineCheck = false
    },
    ["ZyraGraspingRoots"] = {
      charName = "Zyra",
      spellSlot = "E",
      range = 1100,
      width = 70,
      speed = 1400,
      delay = 0.5,
      SpellType = "skillshot",
      collision = true,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = true
    },
    ["ZyraBrambleZone"] = {
      charName = "Zyra",
      spellSlot = "R",
      range = 1100,
      width = 70,
      speed = 20,
      delay = 0.5,
      SpellType = "skillshot",
      collision = false,
      Blockable = false,
      riskLevel = "extreme",
      cc = true,
      hitLineCheck = false
    }
  }
end
