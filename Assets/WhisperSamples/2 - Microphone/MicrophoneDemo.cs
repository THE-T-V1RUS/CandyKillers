using System.Diagnostics;
using TMPro;
using UnityEngine;
using Whisper.Utils;
using FuzzySharp;

namespace Whisper.Samples
{
    /// <summary>
    /// Record audio clip from microphone and make a transcription.
    /// </summary>
    public class MicrophoneDemo : MonoBehaviour
    {
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;
        public EquipmentController equipmentController;
        public bool streamSegments = true;
        public bool printLanguage = true;

        public TextMeshProUGUI outputText;
        
        private string _buffer;
        private bool _recordingCancelled = false;

        private void Awake()
        {
            whisper.OnNewSegment += OnNewSegment;
            whisper.OnProgress += OnProgressHandler;
            
            microphoneRecord.OnRecordStop += OnRecordStop;
        }

        private void OnVadChanged(bool vadStop)
        {
            microphoneRecord.vadStop = vadStop;
        }

        public void ToggleRecording()
        {
            OnButtonPressed();
        }
        
        public void CancelRecording()
        {
            _recordingCancelled = true;
            if (microphoneRecord.IsRecording)
            {
                microphoneRecord.StopRecord();
            }
            _buffer = "";
            if (outputText)
            {
                outputText.text = "Recording cancelled";
            }
        }

        private void OnButtonPressed()
        {
            if (!microphoneRecord.IsRecording)
            {
                _recordingCancelled = false;
                microphoneRecord.StartRecord();
            }
            else
            {
                microphoneRecord.StopRecord();
            }
        }
        
        private async void OnRecordStop(AudioChunk recordedAudio)
        {
            // Don't process results if recording was cancelled
            if (_recordingCancelled)
            {
                _recordingCancelled = false;
                return;
            }
            
            _buffer = "";

            var sw = new Stopwatch();
            sw.Start();
            
            var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
            if (res == null || !outputText) 
                return;

            var time = sw.ElapsedMilliseconds;
            var rate = recordedAudio.Length / (time * 0.001f);

            var text = res.Result;
            
            if (printLanguage)
                text += $"\n\nLanguage: {res.Language}";

            var targetText = equipmentController.GetCurrentPhrase().ToLower();
            var spokenText = text.ToLower();

            //remove special characters from both
            targetText = System.Text.RegularExpressions.Regex.Replace(targetText, "[^a-zA-Z0-9 ]+", "");
            spokenText = System.Text.RegularExpressions.Regex.Replace(spokenText, "[^a-zA-Z0-9 ]+", "");

            var score = Fuzz.Ratio(targetText, spokenText);

            equipmentController.PrayerResults(score);

            text += $"\n\nScore: {score}";

            outputText.text = text;
        }
        
        private void OnLanguageChanged(int ind)
        {
            //var opt = languageDropdown.options[ind];
            //whisper.language = opt.text;
        }
        
        private void OnTranslateChanged(bool translate)
        {
            whisper.translateToEnglish = translate;
        }

        private void OnProgressHandler(int progress)
        {
            return;
        }
        
        private void OnNewSegment(WhisperSegment segment)
        {
            if (!streamSegments || !outputText)
                return;

            _buffer += segment.Text;
            outputText.text = _buffer + "...";
        }
    }
}