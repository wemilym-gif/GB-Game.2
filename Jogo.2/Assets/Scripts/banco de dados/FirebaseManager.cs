using Firebase;
using Firebase.Auth; 
using Firebase.Database;
using Firebase.Extensions; 
using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro; 
using System;

public class FirebaseManager : MonoBehaviour
{
    private string databaseUrl = "https://banco-de-dadosgamebalance-default-rtdb.firebaseio.com/";

    // ==========================================
    // ESTRUTURA DE DADOS DA PARTIDA
    // ==========================================
    [Serializable]
    public class DadosSessao {
        public int pontuacao;
        public int colisoes;
        public float oscilacaoMedia;
        public string dataHora;

        public float pressaoSuperiorEsquerdo;
        public float pressaoSuperiorDireito;
        public float pressaoInferiorEsquerdo;
        public float pressaoInferiorDireito;

        public DadosSessao(int p, int c, float o, float se, float sd, float ie, float id) {
            pontuacao = p;
            colisoes = c;
            oscilacaoMedia = o;
            pressaoSuperiorEsquerdo = se;
            pressaoSuperiorDireito = sd;
            pressaoInferiorEsquerdo = ie;
            pressaoInferiorDireito = id;
            dataHora = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }
    }

    [Header("UI - Login")]
    public TMP_InputField campoEmailLogin;
    public TMP_InputField campoSenhaLogin;
    public TMP_Text textoFeedbackLogin;

    [Header("UI - Cadastro")]
    public TMP_InputField campoEmailCadastro;
    public TMP_InputField campoSenhaCadastro;
    public TMP_Text textoFeedbackCadastro;

    private DatabaseReference reference;
    private FirebaseAuth auth; 
    private FirebaseUser usuarioLogado; 

    public static FirebaseManager Instance { get; private set; }

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available) {
                reference = FirebaseDatabase.GetInstance(databaseUrl).RootReference;
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase inicializado com sucesso!");
            } else {
                Debug.LogError($"Não foi possível inicializar o Firebase: {dependencyStatus}");
            }
        });
    }

    public void CadastrarPaciente()
    {
        if (auth == null) {
            Debug.LogError("Firebase Auth ainda não foi inicializado!");
            return;
        }

        string email = campoEmailCadastro != null ? campoEmailCadastro.text : "";
        string senha = campoSenhaCadastro != null ? campoSenhaCadastro.text : "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha)) {
            if (textoFeedbackCadastro != null) textoFeedbackCadastro.text = "Preencha e-mail e senha!";
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, senha).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted) {
                Debug.LogError("Erro no cadastro: " + task.Exception);
                if (textoFeedbackCadastro != null) textoFeedbackCadastro.text = "Erro ao cadastrar paciente.";
                return;
            }

            usuarioLogado = task.Result.User;
            Debug.Log($"🎉 Paciente cadastrado com sucesso! UID: {usuarioLogado.UserId}");

            if (textoFeedbackCadastro != null) textoFeedbackCadastro.text = "Cadastro realizado!";

            SceneManager.LoadScene("Config - Wii Board");
        });
    }

    public void FazerLoginPaciente()
    {
        if (auth == null) {
            Debug.LogError("Firebase Auth ainda não foi inicializado!");
            return;
        }

        string email = campoEmailLogin != null ? campoEmailLogin.text : "";
        string senha = campoSenhaLogin != null ? campoSenhaLogin.text : "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha)) {
            if (textoFeedbackLogin != null) textoFeedbackLogin.text = "Preencha e-mail e senha!";
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, senha).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted) {
                Debug.LogError("Erro no login: " + task.Exception);
                if (textoFeedbackLogin != null) textoFeedbackLogin.text = "E-mail ou senha incorretos.";
                return;
            }

            usuarioLogado = task.Result.User;
            Debug.Log($"🎉 Login realizado com sucesso! Bem-vindo: {usuarioLogado.Email}");

            if (textoFeedbackLogin != null) textoFeedbackLogin.text = "Login com sucesso!";

            SceneManager.LoadScene("Config - Wii Board");
        });
    }

    public void SalvarPartidaReal(int pontos, int colisoes, float oscilacao, float se, float sd, float ie, float id) 
    {
        if (usuarioLogado == null && auth != null) {
            usuarioLogado = auth.CurrentUser;
        }

        if (reference == null) {
            reference = FirebaseDatabase.GetInstance(databaseUrl).RootReference;
        }

        DadosSessao novaSessao = new DadosSessao(pontos, colisoes, oscilacao, se, sd, ie, id);
        string json = JsonUtility.ToJson(novaSessao);
        string chaveDataHora = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // 1. Salva na pasta 'partidas_recentes' (evita conflito com o nó 'jogadores')
        reference.Child("partidas_recentes")
            .Child(chaveDataHora)
            .SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task => {
                if (task.IsCompleted) {
                    Debug.Log($"🎉 PARTIDA SALVA EM 'partidas_recentes'! Chave: {chaveDataHora}");
                } else if (task.IsFaulted) {
                    Debug.LogError($"Erro ao salvar no Realtime Database: {task.Exception}");
                }
            });

        // 2. Salva também na raiz para manter compatibilidade com chaves antigas
        reference.Child(chaveDataHora)
            .SetRawJsonValueAsync(json);

        // 3. Salva no histórico do jogador logado
        if (usuarioLogado != null) {
            string idPaciente = usuarioLogado.UserId;
            reference.Child("jogadores")
                .Child(idPaciente)
                .Child("historico_sessoes")
                .Child(chaveDataHora)
                .SetRawJsonValueAsync(json);
        }
    }
}