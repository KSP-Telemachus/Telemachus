using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Threading;

namespace Natter
{
    public partial class frmMain : Form
    {
        SpeechRecognitionEngine _recognizer = new SpeechRecognitionEngine();
        static ManualResetEvent _completed = null;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
          
        }

        private void _recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Text == "test") // e.Result.Text contains the recognized text
            {
                Console.WriteLine("The test was successful!");
            }
            else if (e.Result.Text == "exit")
            {
                _completed.Set();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _completed = new ManualResetEvent(false);
            SpeechRecognitionEngine _recognizer = new SpeechRecognitionEngine();
            _recognizer.RequestRecognizerUpdate(); // request for recognizer update
            _recognizer.LoadGrammar(new Grammar(new GrammarBuilder("test"))); // load a grammar
            _recognizer.RequestRecognizerUpdate(); // request for recognizer update
            _recognizer.LoadGrammar(new Grammar(new GrammarBuilder("exit"))); // load a "exit" grammar
            _recognizer.SpeechRecognized += _recognizer_SpeechRecognized;
            _recognizer.SetInputToDefaultAudioDevice(); // set the input of the speech recognizer to the default audio device
            _recognizer.RecognizeAsync(RecognizeMode.Multiple); // recognize speech asynchronous
            _completed.WaitOne(); // wait until speech recognition is completed
            _recognizer.Dispose(); // dispose the speech recognition engine
        }
    }
}
