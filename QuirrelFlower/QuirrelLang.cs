
namespace QuirrelFlower
{
    public static class QuirrelLang
    {
        public const string RepeatCN = "拿着这朵花，我的心情如此平静，愿意陪我在这坐一会吗，小个子朋友?";
        public const string GiveYNCN = "送出娇嫩的花？";
        public const string FlowerOfferCN = "哦，你身上带着一朵漂亮的小花,它似乎有着非同一般的力量";
        public const string AcceptFlowerCN = "这...送给我，真的吗? 谢谢你我的小个子朋友,我会好好珍惜它的";
        public const string DeclineFlowerCN = "这么圣洁的花自己保存着也很不错，谢谢你让我看见这么漂亮的花";
        public const string QuirrelSheet = "Quirrel";
        public const string RepeatKey = "Quirrel_Flower_Repeat";
        public const string GiveKey = "Quirrel_Flower_Give";
        public const string FlowerOfferKey = "Quirrel_Flower_Offer";
        public const string AcceptKey = "Quirrel_Flower_Accept";
        public const string DeclineKey = "Quirrel_Flower_Decline";
        public const string RepeatEN = "What a placid state of mind I have when holding this flower. Would you mind sit down here with me, my little friend?";
        public const string GiveYNEN = "Give out the delicate flower?";
        public const string FlowerOfferEN = "Oh, you carry a beautiful flower with you. There seems to be extraordinary power on it.";
        public const string AcceptFlowerEN = "You want to give it to me... really? I do appreciate the generosity of you, my little friend. I will take good care of it.";
        public const string DeclineFlowerEN = "Not a bad idea to keep the holy flower with you, huh? Thank you for showing me such a beautiful flower.";
        public static Dictionary<string, string> QLanguagesCN = new()
        {
            { RepeatKey, RepeatCN},
            { GiveKey, GiveYNCN},
            { AcceptKey, AcceptFlowerCN},
            { DeclineKey, DeclineFlowerCN},
            { FlowerOfferKey, FlowerOfferCN},
        };
        public static Dictionary<string, string> QLanguagesEN = new()
        {
            { RepeatKey, RepeatEN },
            { GiveKey, GiveYNEN },
            { AcceptKey, AcceptFlowerEN },
            { DeclineKey, DeclineFlowerEN },
            { FlowerOfferKey, FlowerOfferEN },
        };

    }
}
