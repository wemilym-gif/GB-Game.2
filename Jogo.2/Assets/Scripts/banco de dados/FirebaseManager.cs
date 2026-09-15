using Firebase;
using Firebase.Auth; 
using Firebase.Database;
using Firebase.Extensions; 
using UnityEngine;
using UnityEngine.SceneManagement; // Importante para mudar de cena!
using TMPro; 
using System;

public class FirebaseManager : MonoBehaviour
{
    // Estrutura do que será salvo no banco de dados
    [Serializable]
    public class DadosSessao {
        public int pontuacao;
        public int colisoes;
        public float oscilacaoMedia;
        public string data;

        public DadosSessao(int p, int c, float o) {
            pontuacao = p;
            colisoes = c;
            oscilacaoMedia = o;
            data = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }
    }

    private DatabaseReference reference;
    private FirebaseAuth auth; 
    private FirebaseUser usuarioLogado; 

    [Header("Campos do Canvas de Login / Cadastro")]
    public TMP_InputField emailInputField; 
    public TMP_InputField senhaInputField; 

    [Header("Configuração de Cena")]
    public string nomeCenaCalibracao = "Calibracao"; // Digite o nome exato da sua cena aqui

    void Start() {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available) {
                reference = FirebaseDatabase.DefaultInstance.RootReference;
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase pronto para uso!");
            } else {
                Debug.LogError($"Não foi possível inicializar o Firebase: {dependencyStatus}");
            }
        });
    }

    // Função para Cadastrar um Novo Paciente (Criar Conta) e ir para a Calibração
    public void CadastrarPaciente() {
        if (auth == null) return;
        if (emailInputField == null || senhaInputField == null) {
            Debug.LogError("Erro: Arraste os campos de texto no Inspector!");
            return;
        }

        string emailInput = emailInputField.text;
        string senhaInput = senhaInputField.text;

        auth.CreateUserWithEmailAndPasswordAsync(emailInput, senhaInput).ContinueWithOnMainThread(task => {
            if (task.IsFaulted) {
                Debug.LogError("Erro ao cadastrar! Verifique se a senha tem no mínimo 6 caracteres ou se o e-mail já existe.");
                return;
            }

            usuarioLogado = task.Result.User;
            Debug.Log($"🎉 Paciente cadastrado com sucesso! ID: {usuarioLogado.UserId}");

            // Transição automática para a cena de calibração após o cadastro
            IrParaCalibracao();
        });
    }

    // Função para fazer Login do paciente
    public void FazerLogin() {
        if (auth == null) return;
        if (emailInputField == null || senhaInputField == null) {
            Debug.LogError("Erro: Arraste os campos de texto no Inspector!");
            return;
        }

        string emailInput = emailInputField.text;
        string senhaInput = senhaInputField.text;

        auth.SignInWithEmailAndPasswordAsync(emailInput, senhaInput).ContinueWithOnMainThread(task => {
            if (task.IsFaulted) {
                Debug.LogError("Erro ao fazer login! Verifique e-mail e senha.");
                return;
            }
            usuarioLogado = task.Result.User;
            Debug.Log($"Sucesso! Paciente logado com ID: {usuarioLogado.UserId}");

            // Transição para a cena de calibração após o login
            IrParaCalibracao();
        });
    }

    // Função pública para trocar de cena (pode ser chamada diretamente por um botão)
    public void IrParaCalibracao() {
        if (!string.IsNullOrEmpty(nomeCenaCalibracao)) {
            SceneManager.LoadScene(nomeCenaCalibracao);
        } else {
            Debug.LogError("Nome da cena de calibração não foi configurado!");
        }
    }

    // Função chamada quando a partida REAL termina
    public void SalvarPartidaReal(int pontosFinais, int totalColisoes, float oscilacaoCalculada) {
        if (reference == null || usuarioLogado == null) {
            Debug.LogError("Erro: Paciente não está logado! Faça o login antes de jogar.");
            return;
        }

        string idPacienteReal = usuarioLogado.UserId;
        DadosSessao novaSessao = new DadosSessao(pontosFinais, totalColisoes, oscilacaoCalculada);
        string json = JsonUtility.ToJson(novaSessao);

        string chaveDataHora = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        reference.Child("jogadores")
            .Child(idPacienteReal)
            .Child("historico_sessoes")
            .Child(chaveDataHora)
            .SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task => {
                if (task.IsCompleted) {
                    Debug.Log("🎉 DADOS DA PARTIDA REAL SALVOS NO FIREBASE COM SUCESSO!");
                } else if (task.IsFaulted) {
                    Debug.LogError($"Erro ao salvar partida: {task.Exception}");
                }
            });
    }
}