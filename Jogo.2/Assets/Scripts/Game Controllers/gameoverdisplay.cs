using System;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class GameOverDisplay : MonoBehaviour
{
    [Header("Configuração do Firebase")]
    // URL exata do seu banco de dados no Firebase
    public string databaseUrl = "https://banco-de-dadosgamebalance-default-rtdb.firebaseio.com/";

    [Header("Campos Gerais da UI")]
    public TMP_Text textPontuacao;
    public TMP_Text textColisoes;
    public TMP_Text textOscilacao;
    public TMP_Text textDataHora;

    [Header("Campos Separados de Pressão")]
    public TMP_Text textPressaoSuperiorEsquerdo;
    public TMP_Text textPressaoSuperiorDireito;
    public TMP_Text textPressaoInferiorEsquerdo;
    public TMP_Text textPressaoInferiorDireito;

    private DatabaseReference dbRef;

    void OnEnable()
    {
        InicializarEBuscar();
    }

    public void InicializarEBuscar()
    {
        try
        {
            // Passa a URL explicitamente para evitar a falha de conexão no Editor da Unity
            dbRef = FirebaseDatabase.GetInstance(databaseUrl).RootReference;

            // OBS: Se no seu FirebaseManager as partidas são salvas dentro de uma pasta (ex: "partidas"),
            // use a linha abaixo em vez da de cima:
            // dbRef = FirebaseDatabase.GetInstance(databaseUrl).GetReference("partidas");

            CarregarDadosDaUltimaPartida();
        }
        catch (Exception e)
        {
            Debug.LogError("Erro ao inicializar conexão com Firebase: " + e.Message);
        }
    }

    public void CarregarDadosDaUltimaPartida()
    {
        if (dbRef == null) return;

        dbRef.OrderByKey().LimitToLast(1).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Erro ao procurar dados no Firebase: " + task.Exception);
                return;
            }

            if (task.IsCanceled)
            {
                Debug.LogWarning("Busca cancelada.");
                return;
            }

            DataSnapshot snapshot = task.Result;

            if (snapshot != null && snapshot.Exists && snapshot.ChildrenCount > 0)
            {
                foreach (DataSnapshot partida in snapshot.Children)
                {
                    // Leitura das informações gerais
                    string pontuacao = partida.Child("pontuacao").Value?.ToString() ?? "0";
                    string colisoes = partida.Child("colisoes").Value?.ToString() ?? "0";
                    string dataHora = partida.Child("dataHora").Value?.ToString() ?? "-";

                    float oscilacaoVal = ConvertToFloat(partida.Child("oscilacaoMedia").Value);

                    // Leitura individual das pressões
                    float presSupEsq = ConvertToFloat(partida.Child("pressaoSuperiorEsquerdo").Value);
                    float presSupDir = ConvertToFloat(partida.Child("pressaoSuperiorDireito").Value);
                    float presInfEsq = ConvertToFloat(partida.Child("pressaoInferiorEsquerdo").Value);
                    float presInfDir = ConvertToFloat(partida.Child("pressaoInferiorDireito").Value);

                    // Atualização dos textos na UI
                    if (textPontuacao != null) textPontuacao.text = $"Pontuação: {pontuacao}";
                    if (textColisoes != null) textColisoes.text = $"Colisões: {colisoes}";
                    if (textOscilacao != null) textOscilacao.text = $"Oscilação Média: {oscilacaoVal:F2}";
                    if (textDataHora != null) textDataHora.text = $"Data/Hora: {dataHora}";

                    if (textPressaoSuperiorEsquerdo != null) 
                        textPressaoSuperiorEsquerdo.text = $"Pressão no quadrante Superior Esquerdo: {presSupEsq:F2}";

                    if (textPressaoSuperiorDireito != null) 
                        textPressaoSuperiorDireito.text = $"Pressão no quadrante Superior Direito: {presSupDir:F2}";

                    if (textPressaoInferiorEsquerdo != null) 
                        textPressaoInferiorEsquerdo.text = $"Pressão no quadrante Inferior Esquerdo: {presInfEsq:F2}";

                    if (textPressaoInferiorDireito != null) 
                        textPressaoInferiorDireito.text = $"Pressão no quadrante Inferior Direito: {presInfDir:F2}";
                }
            }
            else
            {
                Debug.LogWarning("Nenhuma partida encontrada na base de dados.");
            }
        });
    }

    private float ConvertToFloat(object value)
    {
        if (value == null) return 0f;
        float.TryParse(value.ToString(), out float result);
        return result;
    }
}