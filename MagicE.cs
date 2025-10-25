using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExtraSpell;

internal class StarSpell : Spell //魔法: 星光
{
    public override bool Perform()
    {
        // foreach (var ele in Core.Instance.sources.elements.rows)
        // {
        //     debug.Log($"ExtraSpell: name:{ele.name},alias:{ele.alias}");
        // }
        var power = CC.elements.GetElement(source.aliasParent)?.Value ?? 1;
        ProcAt_Internal(power, source, act);
        var spellExp = CC.Chara.elements.GetSpellExp(CC.Chara, act);
        // var a = CC.Chara.elements.GetElement(source.alias);
        // debug.Log($"ExtraSpell: name:{a.Name} , value:{a.Value}, exp:{a.vExp} , tonext:{a.ExpToNext} ");
        // debug.Log($"ExtraSpell: spellExp:{spellExp}");
        CC.Chara.ModExp(source.alias, spellExp);
        // debug.Log($"ExtraSpell: alias: {source.alias} ,spellExp:{spellExp}");
        return true;
    }

    private static void ProcAt_Internal(int power, SourceElement.Row source, Act act)
    {
        var mainElement = Create(source.aliasRef, power / 10);
        var ro = CC.pos.x == TP.x || CC.pos.z == TP.z;
        var points1 = _map.ListPointsInCircle(TP, source.radius, false, false);
        var points2 = SpellTools.ListPointsInStar(TP, source.radius * 2, _map, ro);
        if (points1.Count == 0)
            points1.Add(TP.Copy());
        if (points2.Count == 0)
            points2.Add(TP.Copy());
        // points2.Add(TP.Copy());

        CC.Chara.Say("spell_ball", CC.Chara, mainElement.Name.ToLower());
        Wait(0.8f, CC.Chara);
        ActEffect.TryDelay(() => CC.Chara.PlaySound("spell_ball"));
        SpellEffects.Atk(CC, TP, power / 10, mainElement, points1, act, source.alias, 0f, true);
        SpellEffects.Atk(CC, TP, power, mainElement, points2, act, source.alias, 0.4f);
        // SpellEffects.Atk(CC, TP, power, mainElement, baseP, "spell_ball", act, 0.4f+0.04f * radius, spellType: "hit_light"); //光爆
        // SpellEffects.Atk(CC, TP, power, mainElement, baseP, "spell_ball", act, 0.4f,spellType: "aura_heaven"); //光柱
    }
}

internal class LightningSpell : Spell //魔法: 雷击
{
    public override bool Perform()
    {
        var power = CC.elements.GetElement(source.aliasParent)?.Value ?? 1;
        ProcAt_Internal(power, source, act);
        var spellExp = CC.Chara.elements.GetSpellExp(CC.Chara, act);
        CC.Chara.ModExp(source.alias, spellExp);
        return true;
    }

    private static void ProcAt_Internal(int power, SourceElement.Row source, Act act)
    {
        var mainElement = Create(source.aliasRef, power / 10);
        var points1 = _map.ListPointsInLine(CC.pos, TP, 99);
        points1.Remove(CC.pos);
        CC.Chara.Say("spell_bolt", CC.Chara, mainElement.Name.ToLower());
        Wait(0.8f, CC.Chara);
        ActEffect.TryDelay(() => CC.Chara.PlaySound("spell_bolt"));
        // 获取目标和延迟
        // 直线目标
        var lineTargets = SpellEffects.Search(points1);
        // 长雷击
        SpellEffects.Atk(CC, TP, power, mainElement, points1, act, source.alias, spellType: "bolt_");
        // 使用队列避免在迭代时复制集合
        var queue = new Queue<KeyValuePair<Point, int>>(lineTargets);
        while (queue.Count > 0)
        {
            var firstT = queue.Dequeue();
            // 遍历直线目标的周围目标
            var targets = _map.ListPointsInCircle(firstT.Key, source.radius, false);
            foreach (var nextT in SpellEffects.Search(targets, firstT.Value))
            {
                // 确认结果未包含新点且不打到施法者
                if (!lineTargets.ContainsKey(nextT.Key) && !Equals(nextT.Key, CC.pos))
                {
                    lineTargets.Add(nextT.Key, nextT.Value);
                    queue.Enqueue(nextT);
                    // 获取连线
                    var lines = _map.ListPointsInLine(firstT.Key, nextT.Key, (int)source.radius + 1);
                    SpellEffects.Atk(CC, firstT.Key, power / Math.Max(nextT.Value / 2, 1), mainElement, lines, act,
                        source.alias, nextT.Value * 0.2f + 0.2f, spellType: "bolt_");
                }
            }
        }
    }
}

/*
internal class BHoleSpell : Spell //魔法: 黑洞
{
    public override bool Perform()
    {
        var power = CC.elements.GetElement(source.aliasParent)?.Value ?? 1;
        ProcAt_Internal(source.radius + power / 100, source.aliasRef);
        var spellExp = CC.Chara.elements.GetSpellExp(CC.Chara, act);
        CC.Chara.ModExp(source.alias, spellExp);
        return true;
    }

    private static void ProcAt_Internal(float radius, string eleRef)
    {
        debug.Log($"ExtraSpell: radius:{radius}");
        var mainElement = Create(eleRef);
        var points1 = _map.ListPointsInCircle(TP, radius, false, false);
        CC.Chara.Say("spell_ball", CC.Chara, mainElement.Name.ToLower());
        Wait(0.8f, CC.Chara);
        ActEffect.TryDelay(() => CC.Chara.PlaySound("spell_ball"));
        SpellEffects.Atk_bh(CC, TP, 1, mainElement, points1, "spell_ball");
    }
}
*/

internal class BlowbackSpell : Spell //魔法: 吹飞
{
    public override bool Perform()
    {
        var power = CC.elements.GetElement(source.aliasParent)?.Value ?? 1;
        ProcAt_Internal(power, source, act);
        var spellExp = CC.Chara.elements.GetSpellExp(CC.Chara, act);
        CC.Chara.ModExp(source.alias, spellExp);
        return true;
    }

    private static void ProcAt_Internal(int power, SourceElement.Row source, Act act)
    {
        var radius = (int)(source.radius + 0.01 * power);
        var mainElement = Create(source.aliasRef, power / 10);
        var points1 = _map.ListPointsInArc(CC.pos, TP, radius, 50);
        points1.Remove(CC.pos);

        CC.Chara.Say("spell_ball", CC.Chara, mainElement.Name.ToLower());
        Wait(0.8f, CC.Chara);
        ActEffect.TryDelay(() => CC.Chara.PlaySound("spell_ball"));
        SpellEffects.Atk(CC, CC.pos, power / 10, mainElement, points1, act, source.alias);

        SpellEffects.Push(CC, points1, power / 20);
    }
}

internal static class SpellEffects //效果
{
    internal static void Atk(Chara cc, Point tp, int power, Element element,
        List<Point> targets, Act act, string alias,
        float exDelay = 0f, bool rev = false, string spellType = "ball_")
    {
        // 默认威力修正（缓存为float避免重复装箱）
        var powerMod = Act.powerMod / 100f;
        // eleP修正，如果不可用默认为 50（保持与原版一致使用整数倍率）
        var elementPowerMod = (act?.ElementPowerMod ?? 50) / 50;

        // 预计算骰子别名，避免多次Split/字符串创建
        var diceAlias = alias ?? "ball_";
        var dice = Dice.Create(diceAlias, power, cc, act);
        if (dice == null && alias != null)
        {
            var idx = alias.IndexOf('_');
            diceAlias = (idx > 0 ? alias.Substring(0, idx) : alias) + "_";
            dice = Dice.Create(diceAlias, power, cc, act);
        }

        // 预计算Effect路径，避免在循环内多次拼接
        string effectPath;
        switch (spellType)
        {
            case "ball_":
            case "bolt_":
                effectPath = "Element/ball_" + (element.id == 0 ? "Void" : element.source.alias.Remove(0, 3));
                break;
            case "hit_light":
            case "aura_heaven":
                effectPath = spellType;
                break;
            default:
                effectPath = "Element/ball_Ether";
                break;
        }

        // 遍历目标点
        foreach (var attackPoint in targets)
        {
            // 加载特效并设置延迟
            var dis = Math.Max(tp.Distance(attackPoint), 1);
            var effect = Effect.Get(effectPath);
            var delay = rev ? 0.25f / dis : 0.04f * dis;
            effect.SetStartDelay(delay + exDelay);
            // 特效播放
            if (spellType == "ball_")
                effect.Play(attackPoint).Flip(attackPoint.x > tp.x); // 播放特效并判断方向
            else
                effect.Play(attackPoint);

            // 骰子启动及伤害计算（复用距离值）
            var randomDamage = dice.Roll();
            var dmg = (int)(powerMod * randomDamage / (0.1f * (9 + dis)));

            // 遍历单位（避免LINQ分配）
            var cards = attackPoint.ListCards();
            foreach (var card in cards)
            {
                if (!(card.isChara || card.trait.CanBeAttacked)) continue;

                // 友军控制与减伤
                if (cc.Chara.IsFriendOrAbove(card.Chara))
                {
                    var controlLv = cc.Evalue(302);
                    if (!cc.IsPC && cc.IsPCFactionOrMinion)
                        controlLv += EClass.pc.Evalue(302);
                    if (controlLv > 0)
                    {
                        if (cc.HasElement(1214)) controlLv *= 2;
                        if (controlLv * 10 > EClass.rnd(dmg + 1))
                        {
                            if (card == card.pos.FirstChara)
                                cc.ModExp(302, cc.IsPC ? 10 : 50);
                            continue;
                        }
                        dmg = EClass.rnd(dmg * 100 / (100 + controlLv * 10 + 1));
                        if (card == card.pos.FirstChara)
                            cc.ModExp(302, cc.IsPC ? 20 : 100);
                    }
                }

                // 应用伤害与仇恨
                card.DamageHP(dmg, element.id, power * elementPowerMod, origin: cc);
                if (card.isChara && !card.Chara.IsPCFactionOrMinion)
                    card.Chara.hostility -= 2;

                // 输出显示
                cc.Say("spell_bolt_hit", cc, card, element.Name.ToLower());
            }

            // 处理地形破坏
            if (attackPoint.HasObj && attackPoint.cell.matObj.hardness <= power / 20)
            {
                EClass._map.MineObj(attackPoint); // 破坏物品
            }
        }
    }

    /*internal static void Atk_bh(Chara cc, Point tp, int power, Element element,
        List<Point> targets, string spellName)
    {
        // 遍历目标点
        foreach (var attackPoint in targets)
        {
            // 加载特效
            var effect = Effect.Get("Element/ball_Darkness");
            // 延迟计算
            var dis = Math.Max(tp.Distance(attackPoint), 1);
            var delay = 0.25f / dis;
            effect.SetStartDelay(delay);
            effect.Play(attackPoint).Flip(attackPoint.x > tp.x); // 播放特效并判断方向

            var dmg = (int)(1 * MagicReprog.SPower.Value) + 2;
            // 攻击点附近的单位
            foreach (var card in attackPoint.ListCards().Where(card => card.isChara || card.trait.CanBeAttacked))
            {
                // 友军
                if (cc == EClass.pc)
                {
                    if (card.Chara.IsPCFactionOrMinion) dmg = 1;
                }
                else if (card.Chara.IsFriendOrAbove())
                    dmg = 1;

                // 应用伤害
                card.DamageHP(dmg, element.id, power, origin: cc);
                // 仇恨
                if (card.isChara)
                    if (dmg > 1)
                    {
                        card.Chara._Move(tp, Card.MoveType.Force);
                        card.Chara.hostility -= 10;
                    }

                // 输出显示
                if (!string.IsNullOrEmpty(spellName))
                {
                    cc.Say(spellName + "_hit", cc, card, element.Name.ToLower());
                }
            }

            // 处理地形破坏
            if (attackPoint.HasObj && attackPoint.cell.matObj.hardness <= power)
            {
                EClass._map.MineObj(attackPoint); // 破坏物品
            }
        }
    }*/

    internal static void Push(Chara cc, List<Point> targets, int er)
    {
        var minPush = Math.Max(er, 2);
        foreach (var attackPoint in targets)
        {
            var cards = attackPoint.ListCards();
            foreach (var card in cards)
            {
                if (!card.isChara) continue;
                for (var i = 0; i < minPush; i++)
                {
                    card.Chara.TryMoveFrom(cc.pos);
                }
            }
        }
    }

    internal static Dictionary<Point, int> Search(List<Point> targets, int num = 0)
    {
        // 预估容量，减少扩容次数
        var refPoints = new Dictionary<Point, int>(targets.Count * 2);
        foreach (var attackPoint in targets)
        {
            var cards = attackPoint.ListCards();
            foreach (var card in cards)
            {
                if (!(card.isChara || card.trait.CanBeAttacked)) continue;
                if (!refPoints.ContainsKey(card.pos))
                {
                    refPoints.Add(card.pos, ++num);
                }
            }
        }
        return refPoints;
    }
}