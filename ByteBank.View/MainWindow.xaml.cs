using ByteBank.Core.Model;
using ByteBank.Core.Repository;
using ByteBank.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ByteBank.View
{
    public partial class MainWindow : Window
    {
        private readonly ContaClienteRepository r_Repositorio;
        private readonly ContaClienteService r_Servico;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
        }

        private void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            var taskSchedulerUI = TaskScheduler.FromCurrentSynchronizationContext();
            BtnProcessar.IsEnabled = false;

            var contas = r_Repositorio.GetContaClientes();

            AtualizarView(new List<string>(), TimeSpan.Zero);

            var inicio = DateTime.Now;

            ConsolidarContas(contas)
                .ContinueWith(task =>
                {
                    var fim = DateTime.Now;
                    var resultado = task.Result;
                    AtualizarView(resultado, fim - inicio);
                }, taskSchedulerUI)
                .ContinueWith(task => {
                    BtnProcessar.IsEnabled = true;
                }, taskSchedulerUI);
        }

        private Task<List<string>> ConsolidarContas(IEnumerable<ContaCliente> contas)
        {
            var resultado = new List<string>();

            var tasks = contas.Select(conta =>
            {
                return Task.Factory.StartNew(() =>
                {
                    var contaResultado = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(contaResultado);
                });
            });

            return Task.WhenAll(tasks).ContinueWith(task =>
            {
                return resultado;
            });
        }

        private void AtualizarView(List<String> result, TimeSpan elapsedTime)
        {
            var tempoDecorrido = $"{ elapsedTime.Seconds }.{ elapsedTime.Milliseconds} segundos!";
            var mensagem = $"Processamento de {result.Count} clientes em {tempoDecorrido}";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;
        }
    }
}