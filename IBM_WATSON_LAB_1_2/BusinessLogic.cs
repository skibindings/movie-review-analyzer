using GoogleCSE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Watson.NaturalLanguageUnderstanding.v1;
using IBM.Watson.NaturalLanguageUnderstanding.v1.Model;
using NUglify;

namespace IBM_WATSON_LAB_1_2
{
    class BusinessLogic
    {
        GoogleSearch gs;
        List<GoogleSearchResult> search_results;
        string search_title;

        IamAuthenticator authenticator;
        NaturalLanguageUnderstandingService naturalLanguageUnderstanding;

        public string imdb_title;
        public string rt_title;
        public string letterboxd_title;

        string imdb_url;
        string rt_url;
        string letterboxd_url;

        public BusinessLogic()
        {
            // настройка 2 апишек, гугл серч (для поисковых запросов, мы ведь по названию должны ссылки ковырнуть на imdx, letterbox и rotten tomatoes
            string cx = "16a9de83dacfa8752";
            string api_key = "AIzaSyBKHVabKL5dFLXa0hIW2mXtch6uubaVQ-Q";

            // настраиваем
            gs = new GoogleSearch(cx, api_key, maxPages: 4, pageSize: 10);

            // тут собсна IBM watson
            // тебе надо будет другую api найти 
            string nlu_url = "https://api.eu-gb.natural-language-understanding.watson.cloud.ibm.com/instances/12c85a54-04ce-4f9f-9cd9-d1f2dceed961";
            string nlu_api_key = "pJs137C8K1udzY4-UE68nzKRXUqUfISPOfMBep_idfxl";

            authenticator = new IamAuthenticator(apikey: nlu_api_key);
            naturalLanguageUnderstanding = new NaturalLanguageUnderstandingService("2020-08-01", authenticator);
            naturalLanguageUnderstanding.SetServiceUrl(nlu_url);

            search_results = null;
            search_title = "";

            imdb_title = "";
            rt_title = "";
            letterboxd_title = "";

            imdb_url = "";
            rt_url = "";
            letterboxd_url = "";
        }

        public void ExtractReviews(string movie_title,
            System.Windows.Forms.Label imdb_em,
            System.Windows.Forms.Label imdb_sent,
            System.Windows.Forms.Label letter_em,
            System.Windows.Forms.Label letter_sent,
            System.Windows.Forms.Label rt_em,
            System.Windows.Forms.Label rt_sent)
        {
            // производим поисковый запрос на imdb, letterboxd, rottentomatoes 
            SearchMovie(movie_title);

            // наши переменные imdb_title, imdb_url и.т.д здесь заполняются (из всех ссылок, выбераем нужные ссылки на фильмы, на этих сайтах)
            // если <ресурс>_title - пустой, значит фильм на этом ресурсе не был найден
            ExtractOnIMDB();
            ExtractOnLetterboxd();
            ExtractOnRT();

            // если найшли фильм на ресурсе, производим анализ по ссылочке 
            if (ExistsOnIMDB())
            {
                AnalyseIMDB(imdb_em, imdb_sent);
            }

            if (ExistsOnRT())
            {
                AnalyseRT(rt_em, rt_sent);
            }

            if (ExistsOnLetterboxd())
            {
                AnalyseLetterboxd(letter_em, letter_sent);
            }
        }

        private void SearchMovie(string title)
        {
            search_title = title;
            search_title = search_title.ToLower();
            search_title = search_title.Trim();

            search_results = gs.Search(search_title);

            foreach (var result in search_results)
            {
                Console.WriteLine(result.Title);
                Console.WriteLine(result.Url);
                Console.WriteLine();
            }
        }

        private void ExtractOnIMDB()
        {
            imdb_title = "";
            imdb_url = "";

            foreach (var result in search_results)
            {
                string title = result.Title;
                string url_real = result.Url;

                string url = url_real.Substring(9); // remove https://
                url = url.TrimEnd('/');

                var url_parts = url.Split('/');
                
                if(url_parts.Length == 3)
                {
                    if(url_parts[1].Equals("title"))
                    {
                        string tt = url_parts[2];
                        if(tt.Substring(0,2).Equals("tt"))
                        {
                            // success
                            imdb_title = title;
                            imdb_url = url_real;
                            if (!imdb_url.EndsWith("/"))
                            {
                                imdb_url = rt_url + "/";
                            }
                            break;
                        }
                    }
                } 
            }
        }

        private void ExtractOnLetterboxd()
        {
            letterboxd_title = "";
            letterboxd_url = "";

            foreach (var result in search_results)
            {
                string title = result.Title;
                string url_real = result.Url;

                string url = url_real.Substring(9); // remove https://
                url = url.TrimEnd('/');

                var url_parts = url.Split('/');

                if (url_parts.Length == 3)
                {
                    if (url_parts[1].Equals("film"))
                    {
                        letterboxd_title = title;
                        letterboxd_url = url_real;
                        if (!letterboxd_url.EndsWith("/"))
                        {
                            letterboxd_url = rt_url + "/";
                        }
                        break;
                    }
                }
            }
        }

        private void ExtractOnRT()
        {
            rt_title = "";
            rt_url = "";

            foreach (var result in search_results)
            {
                string title = result.Title;
                string url_real = result.Url;

                string url = url_real.Substring(9); // remove https://
                url = url.TrimEnd('/');

                var url_parts = url.Split('/');

                if (url_parts.Length == 3)
                {
                    if (url_parts[1].Equals("m"))
                    {
                        rt_title = title;
                        rt_url = url_real;
                        if(!rt_url.EndsWith("/"))
                        {
                            rt_url = rt_url + "/";
                        }
                        break;
                    }
                }
            }
        }

        public bool ExistsOnIMDB()
        {
            return imdb_title.Length > 0;
        }

        public bool ExistsOnLetterboxd()
        {
            return letterboxd_title.Length > 0;
        }

        public bool ExistsOnRT()
        {
            return rt_title.Length > 0;
        }

        private void AnalyseIMDB(
            System.Windows.Forms.Label em,
            System.Windows.Forms.Label sent)
        {
            var result = naturalLanguageUnderstanding.Analyze(
                url: imdb_url+"reviews",
                features: new Features()
                {
                    Emotion = new EmotionOptions()
                    {
                    },

                    Sentiment = new SentimentOptions()
                    {
                    }
                });

            var document_emotions = result.Result.Emotion.Document.Emotion;
            string emotions_string = "";
            emotions_string += "Печаль: " + document_emotions.Sadness.ToString() + "\n";
            emotions_string += "Радость: " + document_emotions.Joy.ToString() + "\n";
            emotions_string += "Страх: " + document_emotions.Fear.ToString() + "\n";
            emotions_string += "Отвращение: " + document_emotions.Disgust.ToString() + "\n";
            emotions_string += "Злость: " + document_emotions.Anger.ToString() + "\n";

            em.Text = emotions_string;

            var document_sentiment = result.Result.Sentiment.Document;
            string sentiment_string = "";
            if(document_sentiment.Label.Equals("positive"))
            {
                sentiment_string += "Позитивное восприятие\n";
            }
            else
            {
                sentiment_string += "Негативное восприятие\n";
            }
            sentiment_string += "Очки: " + document_sentiment.Score.ToString();

            sent.Text = sentiment_string;
        }

        private void AnalyseLetterboxd(
            System.Windows.Forms.Label em,
            System.Windows.Forms.Label sent)
        {
            var result = naturalLanguageUnderstanding.Analyze(
            url: letterboxd_url + "reviews/page/1",
            features: new Features()
            {
                Emotion = new EmotionOptions()
                {
                },

                Sentiment = new SentimentOptions()
                {
                }
            });

            var document_emotions = result.Result.Emotion.Document.Emotion;
            string emotions_string = "";
            emotions_string += "Печаль: " + document_emotions.Sadness.ToString() + "\n";
            emotions_string += "Радость: " + document_emotions.Joy.ToString() + "\n";
            emotions_string += "Страх: " + document_emotions.Fear.ToString() + "\n";
            emotions_string += "Отвращение: " + document_emotions.Disgust.ToString() + "\n";
            emotions_string += "Злость: " + document_emotions.Anger.ToString() + "\n";

            em.Text = emotions_string;

            var document_sentiment = result.Result.Sentiment.Document;
            string sentiment_string = "";
            if (document_sentiment.Label.Equals("positive"))
            {
                sentiment_string += "Позитивное восприятие\n";
            }
            else
            {
                sentiment_string += "Негативное восприятие\n";
            }
            sentiment_string += "Очки: " + document_sentiment.Score.ToString();

            sent.Text = sentiment_string;
        }

        private void AnalyseRT(
            System.Windows.Forms.Label em,
            System.Windows.Forms.Label sent)
        {
            var result = naturalLanguageUnderstanding.Analyze(
            url: rt_url + "reviews?type=user",
            features: new Features()
            {
                Emotion = new EmotionOptions()
                {
                },

                Sentiment = new SentimentOptions()
                {
                }
            });

            var document_emotions = result.Result.Emotion.Document.Emotion;
            string emotions_string = "";
            emotions_string += "Печаль: " + document_emotions.Sadness.ToString() + "\n";
            emotions_string += "Радость: " + document_emotions.Joy.ToString() + "\n";
            emotions_string += "Страх: " + document_emotions.Fear.ToString() + "\n";
            emotions_string += "Отвращение: " + document_emotions.Disgust.ToString() + "\n";
            emotions_string += "Злость: " + document_emotions.Anger.ToString() + "\n";

            em.Text = emotions_string;

            var document_sentiment = result.Result.Sentiment.Document;
            string sentiment_string = "";
            if (document_sentiment.Label.Equals("positive"))
            {
                sentiment_string += "Позитивное восприятие\n";
            }
            else
            {
                sentiment_string += "Негативное восприятие\n";
            }
            sentiment_string += "Очки: " + document_sentiment.Score.ToString();

            sent.Text = sentiment_string;
        }

        // Методы инструменты для тебя
        // Проверка, существует ли веб страница
        private bool WebPageExists(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                response.Close();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }

        // Получение хтмл кода по адресу страницы 
        public String LoadHTML(String url)
        {
            string htmlCode = "";
            using (WebClient client = new WebClient())
            {
                htmlCode = client.DownloadString(url);
            }
            return htmlCode;
        }

        // извлечение из хтмл кода -> текст страницы (там текст твоих отзывов, но не весь)
        public String GetInnerTextOfHTML(String html_str)
        {
            var text = Uglify.HtmlToText(html_str);
            Console.WriteLine(text.ToString());
            return text.ToString();
        }
        // дальше этот текст понадобиться отправить в гугл клауд или любую другую api на анализ эмоций и сентимента
        // к сожалению у тебя чутка больше гемора, так как IBM ватсон по умномы обрабатывает url ( то есть он сам всю работу по извлечению текста делает), а тебе придётся чутка ручками поработать 

        // чистка лишней инфы из текста страницы (текст кнопок и прочего) 
        // метод вряд ли хорошо работает, поэтому лучше его не применять
        public String CropUselessStuff(String inner_text_unprep)
        {
            inner_text_unprep = inner_text_unprep.Substring(1000); // на imdb первые 1000 символов кал ненужный, к примеру
            return inner_text_unprep;
        }
    }
}
