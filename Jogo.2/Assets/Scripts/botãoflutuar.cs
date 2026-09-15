using UnityEngine;

public class BotaoFlutuar : MonoBehaviour
{
    public float velocidade = 3f;  // Velocidade do movimento
    public float amplitude = 10f;  // Distância que o botão se move (altura)

    private Vector3 posicaoInicial;

    void Start()
    {
        // Salva a posição original do botão no Canvas
        posicaoInicial = transform.localPosition;
    }

    void Update()
    {
        // Calcula a nova posição Y usando uma onda senoidal
        float novoY = Mathf.Sin(Time.time * velocidade) * amplitude;
        transform.localPosition = posicaoInicial + new Vector3(0, novoY, 0);
    }
}