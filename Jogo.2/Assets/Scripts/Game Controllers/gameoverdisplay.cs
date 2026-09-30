using System;
using System.Collections;
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

    void OnEnable()
    {
        dadosCarregados = false;
        
        // Exibe "Carregando..." enquanto aguarda a resposta do banco de dados
        ExibirTextoCarregando();

        // Inicia a tentativa contínua de busca
        StartCoroutine(BuscarDadosAteSucesso());
    }

    private void ExibirTextoCarregando()
    {
        if (textPontuacao != null) textPontuacao.text = "Pontuação: Carregando...";
        if (textColisoes != null) textColisoes.text = "Colisões: Carregando...";
        if (textOscilacao != null) textOscilacao.text = "Oscilação Média: Carregando...";
        if (textDataHora != null) textDataHora.text = "Data/Hora: Carregando...";

        if (textPressaoSuperiorEsquerdo != null) 
            textPressaoSuperiorEsquerdo.text = "Pressão no quadrante Superior Esquerdo: Carregando...";
        if (textPressaoSuperiorDireito != null) 
            textPressaoSuperiorDireito.text = "Pressão no quadrante Superior Direito: Carregando...";
        if (textPressaoInferiorEsquerdo != null) 
            textPressaoInferiorEsquerdo.text = "Pressão no quadrante Inferior Esquerdo: Carregando...";
        if (textPressaoInferiorDireito != null) 
            textPressaoInferiorDireito.text = "Pressão no quadrante Inferior Direito: Carregando...";
    }

    private IEnumerator BuscarDadosAteSucesso()
    {
        dbRef = FirebaseDatabase.GetInstance(databaseUrl).RootReference;

        // Enquanto os dados não forem carregados, tenta novamente a cada 1 segundo em tempo real
        while (!dadosCarregados)
        {
            TentarCarregarFirebase();
            yield return new WaitForSecondsRealtime(1.0f);
        }
    }

    private void TentarCarregarFirebase()
    {
        if (dbRef == null) return;

        dbRef.OrderByKey().LimitToLast(1).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning("Aguardando resposta do Firebase...");
                return;
            }

            DataSnapshot snapshot = task.Result;

            if (snapshot != null && snapshot.Exists && snapshot.ChildrenCount > 0)
            {
                foreach (DataSnapshot partida in snapshot.Children)
                {
                    // Lê as informações
                    string pontuacao = partida.Child("pontuacao").Value?.ToString() ?? "0";
                    string colisoes = partida.Child("colisoes").Value?.ToString() ?? "0";
                    string dataHora = partida.Child("dataHora").Value?.ToString() ?? "-";

                    float oscilacaoVal = ConvertToFloat(partida.Child("oscilacaoMedia").Value);

                    float presSupEsq = ConvertToFloat(partida.Child("pressaoSuperiorEsquerdo").Value);
                    float presSupDir = ConvertToFloat(partida.Child("pressaoSuperiorDireito").Value);
                    float presInfEsq = ConvertToFloat(partida.Child("pressaoInferiorEsquerdo").Value);
                    float presInfDir = ConvertToFloat(partida.Child("pressaoInferiorDireito").Value);

                    // Preenche a UI com os dados reais
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

                    // Marca que concluiu para parar o loop da corrotina
                    dadosCarregados = true;
                }
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