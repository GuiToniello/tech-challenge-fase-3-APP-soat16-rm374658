# Pipelines (GitHub Actions)

As pipelines deste repositório testam a aplicação, buildam as 5 imagens das APIs e publicam no Amazon ECR, que é criado manualmente. Depois disso, disparam o restart dos pods no repositório [K8S](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658).

A lógica fica em três **workflows reutilizáveis**, e dois workflows de entrada decidem **quando** chamá-los.

```
.github/workflows/
  _build-test.yml    (workflow_call)      - restore, build e testes da solution .slnx
  _build-push.yml    (workflow_call)      - docker compose build + push das 5 imagens no ECR
  _k8s-restart.yml   (workflow_call)      - dispara o "K8s Apply" (restart-pods=true) no repo K8S
  bootstrap.yml      (workflow_dispatch)  - build-test → build-push → k8s-restart
  deploy.yml         (pull_request, push) - PR: build-test; push na main: build-test → build-push → k8s-restart
```

> Convenção: workflows que começam com `_` são internos e são chamados com `uses: ./.github/workflows/_x.yml`.

## 1. `_build-test.yml`

Contém o job `build-test`, que roda:
1. `actions/checkout@v7`
2. `actions/setup-dotnet@v6` (10.0.x)
3. `dotnet restore`, `dotnet build -c Release` e `dotnet test -c Release` em `server/TechChallenge.Oficina.API.slnx`

Os testes usam banco em memória e mocks. Não precisam de AWS, banco nem Docker, então rodam também em PR de fork.

## 2. `_build-push.yml`

Recebe o input `image-tag`. O Bootstrap e o Deploy passam sempre `latest`, que é a tag fixa dos manifests do repo K8S. Contém o job `build-push`, que roda:
1. **Bloqueio fora da `main`:** falha com erro em vez de publicar imagens de código não revisado.
2. Confere se a variable `ECR_REGISTRY` está configurada.
3. `actions/checkout@v7`, `aws-actions/configure-aws-credentials@v6` e `aws-actions/amazon-ecr-login@v2`.
4. `docker compose build` e `docker compose push` de `api`, `approval-api`, `createos-api`, `getos-api` e `status-api`. Usa o próprio `docker-compose.yml` da raiz, com `REGISTRY_PREFIX=<ECR_REGISTRY>/` e `IMAGE_TAG=<image-tag>`.

Concorrência: na `main` usa o grupo fixo `ecr-push` (`cancel-in-progress: false`), que serializa o push entre Bootstrap e Deploy. O `deploy.yml` também tem concorrência no nível do workflow (`deploy-main`): os pushes na `main` rodam um de cada vez, na ordem de chegada, e um run pendente é substituído pelo mais novo. Assim, um commit antigo não publica `:latest` depois de um mais novo. Em PR, um push novo cancela a verificação anterior.

## 3. `_k8s-restart.yml`

Contém o job `dispatch`, que roda `gh workflow run k8s-apply.yml --repo <K8S_REPO> --ref main -f restart-pods=true`, usando o token do secret `K8S_REPO_TOKEN`.

- **Por que o restart é necessário:** as imagens usam sempre a tag `latest`, então os manifests não mudam. Os pods só puxam a imagem nova depois de um `rollout restart`, que é o que o K8s Apply faz com `restart-pods=true`.
- **O job não espera o K8s Apply terminar.** Acompanhe o resultado nas Actions do repo K8S. Se o cluster ou o RDS estiverem desligados, o K8s Apply de lá falha. As imagens continuam publicadas no ECR.
- **Sem o secret `K8S_REPO_TOKEN`, o job falha com erro.** O `GITHUB_TOKEN` não consegue disparar workflows em outro repositório.

## 4. `bootstrap.yml`

- **Gatilho:** `workflow_dispatch`, a partir da `main`.
- **Jobs:** `build-test` → `build-push` → `k8s-restart`.

Use na primeira publicação das imagens, depois do Bootstrap do repo K8S e do repo DB, e sempre que quiser republicar a HEAD da `main`.

## 5. `deploy.yml`

| Evento | Jobs | Observação |
|---|---|---|
| `pull_request` → `main` (sem filtro de paths) | `build-test` | **Check obrigatório** `build-test / build-test`. Não usa AWS |
| `push` na `main` com mudança em `server/**`, `docker-compose.yml`, `deploy.yml`, `_build-test.yml`, `_build-push.yml` ou `_k8s-restart.yml` | `build-test` → `build-push` → `k8s-restart` | Publica as imagens e reinicia os pods no EKS |

**Não use "Re-run" em um Deploy antigo:** ele publicaria como `latest` a imagem de um commit antigo. Para republicar a HEAD, use o Bootstrap.

---

## Secrets e Variables

Configure em **Settings → Secrets and variables → Actions**:

| Nome | Tipo | Uso |
|---|---|---|
| `AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY` | Secret | Usuário IAM com permissão de push no ECR (veja abaixo) |
| `K8S_REPO_TOKEN` | Secret | Token para disparar o K8s Apply no repo K8S. Use um PAT *fine-grained* restrito ao repo K8S, com **Actions: Read and write** |
| `AWS_REGION` | Variable | `us-east-1` |
| `ECR_REGISTRY` | Variable | `903936907231.dkr.ecr.us-east-1.amazonaws.com` |
| `K8S_REPO` | Variable (opcional) | Padrão `GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658` |

**Permissões IAM mínimas para o push:**
- `ecr:GetAuthorizationToken`, em `*`.
- Em `arn:aws:ecr:us-east-1:903936907231:repository/techchallenge-oficina-*`:
  - `ecr:BatchCheckLayerAvailability`, `ecr:InitiateLayerUpload`, `ecr:UploadLayerPart`, `ecr:CompleteLayerUpload`
  - `ecr:PutImage`, `ecr:BatchGetImage`

Pode ser o mesmo usuário IAM `terraform` dos outros repositórios, se ele tiver essas permissões, ou um usuário dedicado.

**Proteção da `main`** (Settings → Branches): exija Pull Request e o check `build-test / build-test`. Ele aparece na lista depois que o primeiro PR roda.

Todos os callers passam os secrets com `secrets: inherit`, e todos os workflows declaram `permissions: contents: read`.
