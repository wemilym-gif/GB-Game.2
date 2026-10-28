using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class HistoricoManager : MonoBehaviour
{
    private string databaseUrl = "https://banco-de-dadosgamebalance-default-rtdb.firebaseio.com/";

    [Header("Referências da UI")]
    [SerializeField] private Transform containerLista; 
    [SerializeField] private GameObject itemHistoricoPrefab; 

    private DatabaseReference reference;
    private FirebaseAuth auth;

    private void Awake()
    {
        GarantirInicializacao();
    }

    private void GarantirInicializacao()
    {
        if (auth == null) auth = FirebaseAuth.DefaultInstance;
        if (reference == null) reference = FirebaseDatabase.GetInstance(databaseUrl).RootReference;
    }

    public void CarregarHistoricoSessoes()
    {
        GarantirInicializacao();

        // Limpa itens antigos do container
        if (containerLista != null)
        {
            foreach (Transform child in containerLista)
            {
                Destroy(child.gameObject);
            }
        }

        // 1. Diagnóstico de Login
        if (auth == null || auth.CurrentUser == null)
        {
            Debug.LogWarning("⚠️ [HistoricoManager] Nenhum usuário logado no Firebase Auth!");
            return;
        }

        string userId = auth.CurrentUser.UserId;
        Debug.Log($"🔍 [HistoricoManager] Buscando histórico no Firebase para o UID: {userId}");

        // 2. Consulta no Banco de Dados
        reference.Child("jogadores")
                 .Child(userId)
                 .Child("historico_sessoes")
                 .GetValueAsync()
                 .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("❌ [HistoricoManager] Erro na consulta do Firebase: " + task.Exception);
                return;
            }

            if (task.IsCompleted && task.Result.Exists)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log($"✅ [HistoricoManager] {snapshot.ChildrenCount} partida(s) encontrada(s) no Firebase!");

                foreach (DataSnapshot sessionSnapshot in snapshot.Children)
                {
                    string json = sessionSnapshot.GetRawJsonValue();
                    SessaoData sessao = JsonUtility.FromJson<SessaoData>(json);

                    if (itemHistoricoPrefab != null && containerLista != null)
                    {
                        GameObject novoItem = Instantiate(itemHistoricoPrefab, containerLista, false);
                        ItemHistoricoUI scriptItem = novoItem.GetComponent<ItemHistoricoUI>();

                        if (scriptItem != null)
                        {
                            scriptItem.PreencherDados(sessao);
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("⚠️ [HistoricoManager] O nó 'historico_sessoes' não existe para este usuário no banco de dados.");
            }
        });
    }
}