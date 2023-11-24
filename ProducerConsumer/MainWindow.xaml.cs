using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ProducerConsumer
{
    public partial class MainWindow : Window
    {
        private readonly RingBuffer _circularBuffer;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
            _circularBuffer = new RingBuffer(10);
            _cancellationTokenSource = new CancellationTokenSource();
            StartConsumers();
        }

        private void TextInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                _circularBuffer.Enqueue(textBox.Text);
                textBox.TextChanged -= TextInput_TextChanged;
                textBox.Text = string.Empty;
                textBox.TextChanged += TextInput_TextChanged;
            }
        }

        private void StartConsumers()
        {
            Task.Run(() => Consume(_cancellationTokenSource.Token, field1, char.IsLetter));
            Task.Run(() => Consume(_cancellationTokenSource.Token, field2, char.IsDigit));
            Task.Run(() => Consume(_cancellationTokenSource.Token, field3, c => !char.IsLetterOrDigit(c)));
        }

        private void Consume(CancellationToken cancellationToken, TextBox textBox, Predicate<char> predicate)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var data = _circularBuffer.Dequeue(predicate);
                if (!string.IsNullOrEmpty(data))
                {
                    Dispatcher.Invoke(() => textBox.Text += data);
                }
            }
        }
    }
}