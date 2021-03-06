﻿using System.Windows.Forms;
using CoreTweet;
using System.IO;
using System;

namespace SDVXScoreDetector
{
    public class Tweeter
    {
        public static string pinCode = null;

        public static void TweetWithImg(string imgpath, string score)
        {
            var result = MessageBox.Show("リザルト画像とスコアをツイートしますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                var t = Auth();

                if (MainForm.DEBUG)
                {
                    MessageBox.Show("Twitter認証: OK\nAPI Remaining: " + t.Application.RateLimitStatus().RateLimit.Remaining, "DEBUG", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (MessageBox.Show("ツイート送信？", "DEBUG", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        return;
                    }
                }

                var media = t.Media.Upload(media: new FileInfo(imgpath));
                var s = t.Statuses.Update(
                    status: "スコア: " + score,
                    media_ids: new long[] { media.MediaId }
                );

                MessageBox.Show("正常にツイートされました。", "ツイート完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static Tokens Auth()
        {
            var CK = Properties.Settings.Default.CK;
            var CS = Properties.Settings.Default.CS;

            if (Properties.Settings.Default.AK == "" || Properties.Settings.Default.AS == "") {
                // 未認証
                var session = OAuth.Authorize(CK, CS);

                System.Diagnostics.Process.Start(session.AuthorizeUri.AbsoluteUri);
                var getPinForm = new GetPin();
                var ret = getPinForm.ShowDialog();
                if (ret == DialogResult.OK)
                {
                    var t = session.GetTokens(pinCode);
                    Properties.Settings.Default.AK = t.AccessToken;
                    Properties.Settings.Default.AS = t.AccessTokenSecret;
                    Properties.Settings.Default.Save();

                    return t;
                } else
                {
                    throw new ArgumentException("PIN入力画面がキャンセルました");
                }

            } else
            {
                // 認証済み
                var AK = Properties.Settings.Default.AK;
                var AS = Properties.Settings.Default.AS;
                var t = Tokens.Create(CK, CS, AK, AS);
                return t;
            }
        }
    }
}
