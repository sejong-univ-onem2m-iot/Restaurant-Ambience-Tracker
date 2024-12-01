using UnityEngine;

public class CandleFlicker : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float swayAmount = 0.0000001f;    // �¿� ��鸲 ����
    [SerializeField] private float swaySpeed = 2.0f;     // �⺻ ��鸲 �ӵ�

    [Header("Random Movement")]
    [SerializeField] private float randomMovementAmount = 0.05f;  // �ұ�Ģ�� ������ ����
    [SerializeField] private float randomMovementSpeed = 0.2f;    // �ұ�Ģ�� ������ �ӵ�

    [Header("Rotation Settings")]
    [SerializeField] private float tiltAmount = 5.0f;    // ������ ����
    [SerializeField] private float rotationSpeed = 1.5f; // ȸ�� �ӵ�

    // ���� ��ġ�� ȸ���� ����
    private Vector3 startPosition;
    private Quaternion startRotation;

    // ������ ������ (���� �ٸ� �������� ���� ������)
    private float noiseOffset1;
    private float noiseOffset2;
    private float noiseOffset3;

    private void Start()
    {
        // ���� ��ġ�� ȸ���� ����
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;        
        // ������ ������ ���� (���� �ٸ� �к��� �ٸ��� �����̵���)
        noiseOffset1 = Random.Range(0f, 100f);
        noiseOffset2 = Random.Range(0f, 100f);
        noiseOffset3 = Random.Range(0f, 100f);                
    }

    private void Update()
    {
        // �ð��� ���� �⺻ ���� ���̺� ������
        float sineWave = Mathf.Sin(Time.time * swaySpeed);

        // Perlin ����� �̿��� �ұ�Ģ�� ������
        float noiseX = Mathf.PerlinNoise(Time.time * randomMovementSpeed, noiseOffset1) - 0.5f;
        float noiseY = Mathf.PerlinNoise(Time.time * randomMovementSpeed, noiseOffset2) - 0.5f;
        float noiseZ = Mathf.PerlinNoise(Time.time * randomMovementSpeed, noiseOffset3) - 0.5f;

        // ��ġ ���
        Vector3 newPosition = startPosition;
        newPosition.x += sineWave * swayAmount;  // �¿� ��鸲
        newPosition += new Vector3(
            noiseX * randomMovementAmount,
            noiseY * randomMovementAmount,
            noiseZ * randomMovementAmount
        );

        // ȸ�� ���
        Vector3 rotationOffset = new Vector3(
            noiseX * tiltAmount,
            noiseY * tiltAmount,
            sineWave * tiltAmount
        );

        // ��ġ�� ȸ�� ����
        transform.localPosition = newPosition;
        transform.localRotation = startRotation * Quaternion.Euler(rotationOffset);
    }

    // ���� ���� �� �Ķ���� ������ ���� public �޼����
    public void SetSwayAmount(float amount) => swayAmount = amount;
    public void SetSwaySpeed(float speed) => swaySpeed = speed;
    public void SetRandomMovement(float amount) => randomMovementAmount = amount;
    public void SetTiltAmount(float amount) => tiltAmount = amount;
}