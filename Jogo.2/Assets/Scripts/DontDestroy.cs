using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    public static DontDestroy Instancia { get; private set; }

    private void Awake()
    {
        // 1. SE JÁ EXISTE UMA INSTÂNCIA E NÃO SOU EU:
        if (Instancia != null && Instancia != this)
        {
            // Destrói a cópia nova que acabou de nascer na nova cena
            Destroy(gameObject);
            return; // Impede que o resto do código deste script rode
        }

        // 2. SE FOR O PRIMEIRO OBJETO CRIADO:
        Instancia = this; // Torna-se o objeto oficial
        DontDestroyOnLoad(gameObject); // Garante que ele sobreviva entre as fases

    }
}
