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
    // ==========================================
    // ESTRUTURA DE DADOS DA PARTIDA
    // ==========================================
    [Serializable]
    public class DadosSessao {
        public int pontuacao;
        public int colisoes;
        public float oscilacaoMedia;
        public string dataHora;

        // Dados específicos dos 4 quadrantes da Wii Balance Board / Arduino
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

    // ==========================================
    // REFERÊNCIAS DA UI - LOGIN
    // ==========================================
    [Header("UI - Login")]
    public TMP_InputField campoEmailLogin;
    public TMP_InputField campoSenhaLogin;
    public TMP_Text textoFeedbackLogin;

    // ==========================================
    // REFERÊNCIAS DA UI - CADASTRO
    // ==========================================
    [Header("UI - Cadastro")]
    public TMP_InputField campoEmailCadastro;
    public TMP_InputField campoSenhaCadastro;
    public TMP_Text textoFeedbackCadastro;

    // ==========================================
    // VARIÁVEIS INTERNAS DO FIREBASE
    // ==========================================
    private DatabaseReference reference;
    private FirebaseAuth auth; 
    private FirebaseUser usuarioLogado; 

    // Singleton
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
                reference = FirebaseDatabase.DefaultInstance.RootReference;
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase inicializado com sucesso!");
            } else {
                Debug.LogError($"Não foi possível inicializar o Firebase: {dependencyStatus}");
            }
        });
    }

    // ==========================================
    // MÉTODO: CADASTRO DE PACIENTE (BOTÃO)
    // ==========================================
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

            // Avança para a próxima cena após o cadastro
            SceneManager.LoadScene("Config - Wii Board");
        });
    }

    // ==========================================
    // MÉTODO: LOGIN DE PACIENTE (BOTÃO)
    // ==========================================
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

            // Avança para a próxima cena após o login
            SceneManager.LoadScene("Config - Wii Board");
        });
    }

    // ==========================================
    // MÉTODO: SALVAR DADOS DA PARTIDA
    // ==========================================
    public void SalvarPartidaReal(int pontos, int colisoes, float oscilacao, float se, float sd, float ie, float id) 
    {
        if (usuarioLogado == null && auth != null) {
            usuarioLogado = auth.CurrentUser;
        }

        if (reference == null || usuarioLogado == null) {
            Debug.LogError("Erro: Nenhum paciente está logado para salvar a partida!");
            return;
        }

        string idPaciente = usuarioLogado.UserId;
        DadosSessao novaSessao = new DadosSessao(pontos, colisoes, oscilacao, se, sd, ie, id);
        string json = JsonUtility.ToJson(novaSessao);

        string chaveDataHora = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        reference.Child("jogadores")
            .Child(idPaciente)
            .Child("historico_sessoes")
            .Child(chaveDataHora)
            .SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task => {
                if (task.IsCompleted) {
                    Debug.Log($"🎉 PARTIDA SALVA! Pontos: {pontos} | Data: {novaSessao.dataHora}");
                } else if (task.IsFaulted) {
                    Debug.LogError($"Erro ao salvar no Realtime Database: {task.Exception}");
                }
            });
    }
}