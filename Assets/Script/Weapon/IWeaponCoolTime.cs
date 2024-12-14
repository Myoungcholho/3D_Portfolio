public interface IWeaponCoolTime
{
    float GetQSkillCooldown();
    float GetQSkillCoolRemaining();

    float GetESkillCooldown();
    float GetESkillCoolRemaining();

    // ¸®´º¾ó
    float GetSkillCooldown(SkillType2 skillType);
    float GetSkillCoolRemaining(SkillType2 skillType);
}
