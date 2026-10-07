using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Extensions;

public class GameOverDisplay : MonoBehaviour
{
    private string databaseUrl = "https://banco-de-dadosgamebalance-default-rtdb.firebaseio.com/";

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
    private bool dadosCarregados = false;
    private bool buscandoDados = false;

    void OnEnable()
    {
        dadosCarregados = false;
        buscandoDados = false;
        
        ExibirTextoCarregando();
        StartCoroutine(BuscarDadosAteSucesso());
    }

    private void ExibirTextoCarregando()
    {
        if (textPontuacao != null) textPontuacao.text = "Pontuação: Carregando...";
        if (textColisoes != null) textColisoes.text = "Colisões: Carregando...";
        if (textOscilacao != null) textOscilacao.text = "Oscilação Média: Carregando...";
        if (textDataHora != null) textDataHora.text = "Data/Hora: Carregando...";

        if (textPressaoSuperiorEsquerdo != null) 
            textPressaoSuperiorEsquerdo.text = "Pressão Sup. Esquerdo: Carregando...";
        if (textPressaoSuperiorDireito != null) 
            textPressaoSuperiorDireito.text = "Pressão Sup. Direito: Carregando...";
        if (textPressaoInferiorEsquerdo != null) 
            textPressaoInferiorEsquerdo.text = "Pressão Inf. Esquerdo: Carregando...";
        if (textPressaoInferiorDireito != null) 
            textPressaoInferiorDireito.text = "Pressão Inf. Direito: Carregando...";
    }

    private IEnumerator BuscarDadosAteSucesso()
    {
        // Aponta para a pasta 'partidas_recentes'
        dbRef = FirebaseDatabase.GetInstance(databaseUrl).RootReference.Child("partidas_recentes");

        while (!dadosCarregados)
        {
            if (!buscandoDados)
            {
                TentarCarregarFirebase();
            }
            yield return new WaitForSecondsRealtime(1.0f);
        }
    }

    private void TentarCarregarFirebase()
    {
        if (dbRef == null) return;

        buscandoDados = true;

        dbRef.OrderByKey().LimitToLast(1).GetValueAsync().ContinueWithOnMainThread(task => {
            buscandoDados = false;

            if (task.IsFaulted || task.IsCanceled)
            {
                TentarCarregarRaizFallback();
                return;
            }

            DataSnapshot snapshot = task.Result;

            if (snapshot != null && snapshot.Exists && snapshot.ChildrenCount > 0)
            {
                ProcessarSnapshot(snapshot);
            }
            else
            {
                // Tenta a raiz caso a pasta 'partidas_recentes' ainda esteja vazia
                TentarCarregarRaizFallback();
            }
        });
    }

    private void TentarCarregarRaizFallback()
    {
        var rootRef = FirebaseDatabase.GetInstance(databaseUrl).RootReference;
        rootRef.GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted && task.Result != null && task.Result.Exists)
            {
                DataSnapshot ultimaPartida = null;
                foreach (DataSnapshot child in task.Result.Children)
                {
                    // Ignora as pastas do sistema para pegar apenas os timestamps soltos na raiz
                    if (child.Key != "jogadores" && child.Key != "partidas_recentes")
                    {
                        ultimaPartida = child;
                    }
                }

                if (ultimaPartida != null)
                {
                    PreencherUI(ultimaPartida);
                    dadosCarregados = true;
                }
            }
        });
    }

    private void ProcessarSnapshot(DataSnapshot snapshot)
    {
        foreach (DataSnapshot partida in snapshot.Children)
        {
            PreencherUI(partida);
            dadosCarregados = true;
        }
    }

    private void PreencherUI(DataSnapshot partida)
    {
        string pontuacao = partida.Child("pontuacao").Value?.ToString() ?? "0";
        string colisoes = partida.Child("colisoes").Value?.ToString() ?? "0";
        string dataHora = partida.Child("dataHora").Value?.ToString() ?? "-";

        float oscilacaoVal = ConvertToFloat(partida.Child("oscilacaoMedia").Value);

        float presSupEsq = ConvertToFloat(partida.Child("pressaoSuperiorEsquerdo").Value);
        float presSupDir = ConvertToFloat(partida.Child("pressaoSuperiorDireito").Value);
        float presInfEsq = ConvertToFloat(partida.Child("pressaoInferiorEsquerdo").Value);
        float presInfDir = ConvertToFloat(partida.Child("pressaoInferiorDireito").Value);

        if (textPontuacao != null) textPontuacao.text = $"Pontuação: {pontuacao}";
        if (textColisoes != null) textColisoes.text = $"Colisões: {colisoes}";
        if (textOscilacao != null) textOscilacao.text = $"Oscilação Média: {oscilacaoVal:F2}";
        if (textDataHora != null) textDataHora.text = $"Data/Hora: {dataHora}";

        if (textPressaoSuperiorEsquerdo != null) 
            textPressaoSuperiorEsquerdo.text = $"Pressão Sup. Esquerdo: {presSupEsq:F2}";

        if (textPressaoSuperiorDireito != null) 
            textPressaoSuperiorDireito.text = $"Pressão Sup. Direito: {presSupDir:F2}";

        if (textPressaoInferiorEsquerdo != null) 
            textPressaoInferiorEsquerdo.text = $"Pressão Inf. Esquerdo: {presInfEsq:F2}";

        if (textPressaoInferiorDireito != null) 
            textPressaoInferiorDireito.text = $"Pressão Inf. Direito: {presInfDir:F2}";
    }

    private float ConvertToFloat(object value)
    {
        if (value == null) return 0f;

        if (float.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
        {
            return result;
        }
        return 0f;
    }
}