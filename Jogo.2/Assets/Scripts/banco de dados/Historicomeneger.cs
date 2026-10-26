using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class HistoricoManager : MonoBehaviour
{
    [Header("Referências da UI")]
    [SerializeField] private Transform containerLista; 
    [SerializeField] private GameObject itemHistoricoPrefab; 

    private DatabaseReference reference;
    private FirebaseAuth auth;

    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;
    }

   
    public void CarregarHistoricoSessoes()
    {
    
        foreach (Transform child in containerLista)
        {
            Destroy(child.gameObject);
        }

     
        if (auth.CurrentUser == null)
        {
            Debug.LogWarning("[HistoricoManager] Nenhum paciente logado no momento.");
            return;
        }

        string userId = auth.CurrentUser.UserId;

      
        reference.Child("partidas_recentes").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("[HistoricoManager] Erro ao carregar histórico: " + task.Exception);
                return;
            }

            if (task.IsCompleted && task.Result.Exists)
            {
                DataSnapshot snapshot = task.Result;

               
                foreach (DataSnapshot sessionSnapshot in snapshot.Children)
                {
                    string json = sessionSnapshot.GetRawJsonValue();
                    SessaoData sessao = JsonUtility.FromJson<SessaoData>(json);

                    // Instancia o prefab da linha dentro do 'Content'
                    GameObject novoItem = Instantiate(itemHistoricoPrefab, containerLista);
                    ItemHistoricoUI scriptItem = novoItem.GetComponent<ItemHistoricoUI>();

                    if (scriptItem != null)
                    {
                        scriptItem.PreencherDados(sessao);
                    }
                }
            }
            else
            {
                Debug.Log("[HistoricoManager] Nenhuma partida encontrada para este paciente.");
            }
        });
    }
}