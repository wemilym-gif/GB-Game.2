using Firebase;
using Firebase.Auth; 
using Firebase.Database;
using Firebase.Extensions; 
using UnityEngine;
using TMPro; // Biblioteca para os campos de texto da UI
using System;

public class FirebaseManager : MonoBehaviour
{
    
    [Serializable]
    public class DadosSessao {
        public int pontuacao;
        public float oscilacaoMedia;
        public string data;

        public DadosSessao(int p, float o) {
            pontuacao = p;
            oscilacaoMedia = o;
            data = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }
    }

    private DatabaseReference reference;
    private FirebaseAuth auth; 
    private FirebaseUser usuarioLogado; 

    [Header("Componentes Visuais da UI")]
    public TMP_InputField emailInputField; // Caixa onde o usuário digita o e-mail
    public TMP_InputField senhaInputField; // Caixa onde o usuário digita a senha

    [Header("Dados de Teste da Partida")]
    public int pontosParaSalvar = 150;
    public float oscilacaoParaSalvar = 1.45f;

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

    public void FazerLogin() {
        if (auth == null) {
            Debug.LogError("Firebase Auth ainda não inicializou!");
            return;
        }

        // Pega o texto de dentro dos componentes visuais da tela
        string emailInput = emailInputField.text;
        string senhaInput = senhaInputField.text;

        if (string.IsNullOrEmpty(emailInput) || string.IsNullOrEmpty(senhaInput)) {
            Debug.LogWarning("Por favor, preencha o e-mail e a senha na tela!");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(emailInput, senhaInput).ContinueWithOnMainThread(task => {
            if (task.IsCanceled) {
                Debug.LogError("Login cancelado.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("Erro ao fazer login: E-mail ou senha incorretos.");
                return;
            }

            usuarioLogado = task.Result.User;
            Debug.Log($"Sucesso! Paciente logado. ID do Firebase: {usuarioLogado.UserId}");
        });
    }

    public void SalvarPartida() {
        if (reference == null || usuarioLogado == null) {
            Debug.LogError("Erro: Firebase não inicializado ou nenhum paciente está logado!");
            return;
        }

        string idPacienteReal = usuarioLogado.UserId;
        DadosSessao novaSessao = new DadosSessao(pontosParaSalvar, oscilacaoParaSalvar);
        string json = JsonUtility.ToJson(novaSessao);

        string key = reference.Child("jogadores").Child(idPacienteReal).Child("historico_sessoes").Push().Key;

        reference.Child("jogadores")
            .Child(idPacienteReal)
            .Child("historico_sessoes")
            .Child(key)
            .SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task => {
                if (task.IsFaulted) {
                    Debug.LogError("Erro ao salvar dados: " + task.Exception);
                } else if (task.IsCompleted) {
                    Debug.Log($"Sucesso! Dados guardados de forma segura no ID: {idPacienteReal}");
                }
            });
    }                                                                                                                                                                      
}