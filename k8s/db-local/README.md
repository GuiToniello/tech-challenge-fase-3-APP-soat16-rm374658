# Manifesto do Postgres para k8s local

Os manifestos desta pasta servem apenas para rodar o banco de dados `Postgres` em um Kubernetes local.

Na nuvem (`AWS`), eles não devem ser usados. Lá o banco é o Amazon RDS do repositório [DB](https://github.com/GuiToniello/tech-challenge-fase-3-DB-soat16-rm374658).

## Recursos criados

1. Deployment com a imagem `postgres:17-alpine`, com probes de readiness e liveness via `pg_isready`.
2. Service `ClusterIP` chamado `postgres`, na porta `5432`.
3. ConfigMap com `POSTGRES_DB` e `POSTGRES_USER`.
4. Secret com `POSTGRES_PASSWORD` (senha só de desenvolvimento local).
5. PVC para persistir os dados em `/var/lib/postgresql/data`.

## Como aplicar

Os manifestos usam o namespace `oficina`. Os manifests das APIs, que ficam no repositório [K8S](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658), já criam esse namespace. Sem eles, crie o namespace antes:

```powershell
kubectl create namespace oficina
kubectl apply -f k8s/db-local
kubectl get pods,svc,pvc -n oficina
```

## Connection string das APIs no k8s local

As APIs acessam o Postgres pelo DNS interno do cluster, usando o host `postgres`. Para rodar localmente os manifests do repositório K8S, preencha o `k8s/.env` de lá com:

```
DatabaseSettings__ConnectionString=Host=postgres;Port=5432;Database=oficina;Username=sa;Password=P@ssw0rd123
```
