using Microsoft.ProjectOxford.Emotion;
using System;
using System.IO;
using System.Linq;
using EmotionRic.Models;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Emotion.Contract;
//using Microsoft.ProjectOxford.Common.Contract;

namespace EmotionRic.Util
{
    public class EmotionHelper
    {
        public EmotionServiceClient emoCliente;

        public EmotionHelper(string key)
        {
            emoCliente = new EmotionServiceClient(key);     
        }

        public async Task<EmoPicture> DetectAndExtractFacesAsync(Stream imageStream)
        {
            Emotion[] emotions =  await emoCliente.RecognizeAsync(imageStream);

            var emoPicture = new EmotionRic.Models.EmoPicture();

            emoPicture.Faces = ExtraxtFaces(emotions, emoPicture);

            return emoPicture;
        }

        private ObservableCollection<EmoFace> ExtraxtFaces(Emotion[] emotions, EmoPicture emoPicture)
        {
            var ListaFaces = new ObservableCollection<EmoFace>();
            foreach (var emotion in emotions)
            {
                var emoFace = new EmoFace()
                {
                    X = emotion.FaceRectangle.Left,
                    Y = emotion.FaceRectangle.Top,
                    Width = emotion.FaceRectangle.Width,
                    Height = emotion.FaceRectangle.Height,
                    Picture = emoPicture
                };

                emoFace.Emotions = ProcessEmotions(emotion.Scores, emoFace);
                ListaFaces.Add(emoFace);  

            }

            return ListaFaces;

        }


        private ObservableCollection<EmoEmotion> ProcessEmotions(Microsoft.ProjectOxford.Common.Contract.EmotionScores  scores, EmoFace  emoFace)
        {
            var emotionList = new ObservableCollection<EmoEmotion>();

            var properties = scores.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            // var filterProperties = properties.Where(p => p.PropertyType == typeof(float));
            var filterProperties = from prop in properties
                                   where prop.PropertyType == typeof(float)
                                   select prop;

            var emotype = EmoEmotionEnum.Untertermind;
            foreach (var propertie in filterProperties)
            {
                if (!Enum.TryParse<EmoEmotionEnum>(propertie.Name, out emotype))
                    emotype = EmoEmotionEnum.Untertermind;

                var emoEmotion = new EmoEmotion();
                emoEmotion.Score = (float) propertie.GetValue(scores);
                emoEmotion.EmotionType = emotype;
                emoEmotion.Face = emoFace;

                emotionList.Add(emoEmotion);
            }

            return emotionList;
        }
    }
}