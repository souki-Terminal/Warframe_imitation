using System.Collections.Generic;

public enum WordCategory
{
    Fish,
    Vegetable, // 追加
    Country,   // 追加
    Vehicle    // 追加
}

public static class TypingWordProvider
{
    public static List<string> GetWordList(WordCategory category, int totalEnhancements)
    {
        List<string> fullList = new List<string>();

        switch (category)
        {
            case WordCategory.Fish:
                fullList = new List<string> 
                {
                    "まぐろ", "さけ", "さんま", "たい", "ひらめ", "かれい", "あじ", "いわし", "さば", "かつお", 
                    "ぶり", "はまち", "かんぱち", "すずき", "うなぎ", "あなご", "ふぐ", "たら", "さわら", "ししゃも", 
                    "にしん", "わかさぎ", "あゆ", "こい", "ふな", "きんぎょ", "めだか", "どじょう", "なまず", "うつぼ", 
                    "えい", "さめ", "まんぼう", "たつのおとしご", "あんこう", "うに", "かに", "えび", "いか", "たこ"
                };
                break;
            case WordCategory.Vegetable:
                fullList = new List<string> 
                {
                    "にんじん", "たまねぎ", "じゃがいも", "きゃべつ", "れタす", "とまと", "きゅうり", "なす", "ぴーまん", "ねぎ",
                    "はくさい", "だいこん", "ごぼう", "さつまいも", "ほうれんそう", "こまつな", "にら", "にんにく", "しょうが", "かぼちゃ",
                    "とうもろこし", "えだまめ", "そらまめ", "あすぱらがす", "ぶろっこりー", "かりふらわー", "せるり", "ぱせり", "みつば", "しゅんぎく",
                    "たけのこ", "れんこん", "やまも", "さといも", "もやし", "きのこ", "しいたけ", "えのき", "しめじ", "まいたけ"
                };
                // カタカナ混じりのミスを修正
                fullList[4] = "れたす";
                fullList[26] = "せろり";
                fullList[32] = "やまいも";
                break;
            case WordCategory.Country:
                fullList = new List<string> 
                {
                    "にほん", "あめりか", "かなだ", "いぎりす", "ふらんす", "どいつ", "いたりあ", "すぺいん", "すいす", "おらんだ",
                    "べらるーし", "ろしあ", "ちゅうごく", "かんこく", "たいわん", "いんど", "おーすとらりあ", "ぶらじる", "あるぜんちん", "めきしこ",
                    "えじぷと", "もろっこ", "みなみあふりか", "けにあ", "ないじぇりあ", "さうじあらびあ", "とるこ", "いらん", "いらく", "いすらえる",
                    "しんがぽーる", "まれーしあ", "たい", "べとなむ", "ふぃりぴん", "いんどねしあ", "にゅーじーらんど", "ぺるー", "ちり", "ころんびあ"
                };
                break;
            case WordCategory.Vehicle:
                fullList = new List<string> 
                {
                    "くるま", "ばす", "たくしー", "とらっく", "きゅうきゅうしゃ", "しょうぼうしゃ", "ぱとかー", "ごみしゅうしゅうしゃ", "ばいく", "じてんしゃ",
                    "でんしゃ", "しんかんせん", "ちかてつ", "ろめんでんしゃ", "もどれーる", "けーぶるかー", "ろーぷうぇい", "ひこうき", "へりこぷたー", "ききゅう",
                    "ふね", "よっと", "ぼーと", "せんすいかん", "きゃくせん", "かもつせん", "ふぇりー", "たんかー", "ほばーくらふと", "ろけっと",
                    "すぺーすしゃとる", "じんりきしゃ", "ばしゃ", "さんりんしゃ", "いちりんしゃ", "くるまいす", "うばぐるま", "しろばい", "すくーたー", "とらくたー"
                };
                // モノレール修正
                fullList[14] = "ものれーる";
                break;
            default:
                fullList = new List<string> { "てすと" };
                break;
        }

        // 難易度に応じた文字数の制限
        // レベル1：強化回数0～2回 → 2～3文字
        // レベル2：強化回数3～5回 → 2～5文字
        // レベル3：強化回数5以上（6回以上） → すべての文字（制限なし）
        int minLen = 2;
        int maxLen = 999;

        if (totalEnhancements <= 2)
        {
            maxLen = 3;
        }
        else if (totalEnhancements <= 5)
        {
            maxLen = 5;
        }

        List<string> filteredList = new List<string>();
        foreach (string word in fullList)
        {
            if (word.Length >= minLen && word.Length <= maxLen)
            {
                filteredList.Add(word);
            }
        }

        // もし条件に合う単語が一つもなかった場合のフォールバック
        if (filteredList.Count == 0)
        {
            filteredList.Add("あ"); // 最低1文字は返す
        }

        return filteredList;
    }
}
