<script setup lang="ts">
import api from "@/api/api";
import AppIcon from "@/components/AppIcon.vue";
import type { CreateTodoRequest, Todo, UpdateTodoRequest } from "@/models/todo";
import { computed, nextTick, onMounted, ref, watch } from "vue";
import { useRouter } from "vue-router";

const router = useRouter();
const todos = ref<Todo[]>([]);
const filter = ref<"all" | "active" | "completed">("all");
const search = ref("");
const loading = ref(true);
const saving = ref(false);
const busyId = ref<number | null>(null);
const error = ref("");
const notice = ref("");
const id = ref(0);
const title = ref("");
const description = ref("");
const isActive = ref(true);
const createDate = ref<Date>(new Date());
const titleInput = ref<HTMLInputElement | null>(null);
const deleteTarget = ref<Todo | null>(null);
const deleteDialog = ref<HTMLDialogElement | null>(null);
watch(deleteTarget, async (todo) => {
  if (todo) {
    await nextTick();
    deleteDialog.value?.showModal();
  } else {
    deleteDialog.value?.close();
  }
});
const activeCount = computed(
  () => todos.value.filter((todo) => todo.isActive).length,
);
const completedCount = computed(() => todos.value.length - activeCount.value);
const progress = computed(() =>
  todos.value.length
    ? Math.round((completedCount.value / todos.value.length) * 100)
    : 0,
);
const visibleTodos = computed(() =>
  todos.value.filter(
    (todo) =>
      (filter.value === "all" ||
        (filter.value === "active" ? todo.isActive : !todo.isActive)) &&
      `${todo.title} ${todo.description}`
        .toLocaleLowerCase("tr")
        .includes(search.value.toLocaleLowerCase("tr")),
  ),
);
const today = new Intl.DateTimeFormat("tr-TR", {
  day: "numeric",
  month: "long",
  year: "numeric",
  weekday: "long",
}).format(new Date());
function formatDate(value: Date) {
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? "Tarih belirtilmedi"
    : new Intl.DateTimeFormat("tr-TR", {
        day: "numeric",
        month: "short",
      }).format(date);
}
async function loadTodos() {
  loading.value = true;
  error.value = "";
  try {
    todos.value = (await api.get<Todo[]>("/Todos")).data;
  } catch {
    error.value =
      "Görevler yüklenemedi. Bağlantınızı kontrol edip yeniden deneyin.";
  } finally {
    loading.value = false;
  }
}
function clearForm() {
  id.value = 0;
  title.value = "";
  description.value = "";
  isActive.value = true;
}
async function focusForm() {
  await nextTick();
  titleInput.value?.focus();
  titleInput.value?.scrollIntoView({ behavior: "smooth", block: "center" });
}
function newTodo() {
  clearForm();
  focusForm();
}
function editTodo(todo: Todo) {
  id.value = todo.id;
  title.value = todo.title;
  description.value = todo.description;
  isActive.value = todo.isActive;
  createDate.value = todo.createDate;
  focusForm();
}
async function saveTodo() {
  if (!title.value.trim() || saving.value) return;
  saving.value = true;
  error.value = "";
  notice.value = "";
  try {
    const request: CreateTodoRequest = {
      title: title.value.trim(),
      description: description.value.trim(),
      isActive: isActive.value,
      createDate: new Date(),
    };
    if (id.value) {
      const update: UpdateTodoRequest = {
        ...request,
        createDate: createDate.value,
        updateDate: new Date(),
      };
      await api.put(`/Todos/${id.value}`, update);
    } else {
      await api.post("/Todos", request);
    }
    notice.value = id.value
      ? "Görev güncellendi."
      : "Yeni görev eklendi. Hadi başlayalım!";
    clearForm();
    await loadTodos();
  } catch {
    error.value = "Görev kaydedilemedi. Lütfen yeniden deneyin.";
  } finally {
    saving.value = false;
  }
}
async function toggleTodo(todo: Todo) {
  if (busyId.value !== null) return;
  busyId.value = todo.id;
  error.value = "";
  try {
    await api.put(`/Todos/${todo.id}`, {
      title: todo.title,
      description: todo.description,
      isActive: !todo.isActive,
      createDate: todo.createDate,
      updateDate: new Date(),
    } satisfies UpdateTodoRequest);
    todo.isActive = !todo.isActive;
  } catch {
    error.value = "Görev durumu değiştirilemedi. Lütfen yeniden deneyin.";
  } finally {
    busyId.value = null;
  }
}
async function deleteTodo() {
  const todo = deleteTarget.value;
  if (!todo || busyId.value !== null) return;
  busyId.value = todo.id;
  error.value = "";
  try {
    await api.delete(`/Todos/${todo.id}`);
    todos.value = todos.value.filter((item) => item.id !== todo.id);
    if (id.value === todo.id) clearForm();
    deleteTarget.value = null;
    notice.value = "Görev silindi.";
  } catch {
    deleteTarget.value = null;
    error.value = "Görev silinemedi. Lütfen yeniden deneyin.";
  } finally {
    busyId.value = null;
  }
}
function logout() {
  localStorage.removeItem("token");
  router.push("/login");
}
onMounted(loadTodos);
</script>

<template>
  <div class="workspace">
    <aside class="sidebar">
      <RouterLink to="/todos" class="brand"
        ><span class="brand-icon"><AppIcon name="check" :size="23" /></span>
        odak<span class="brand-dot">.</span></RouterLink
      >
      <div class="workspace-label">KİŞİSEL ALANIN</div>
      <nav aria-label="Görev menüsü">
        <button
          :class="['nav-item', { selected: filter === 'all' }]"
          @click="filter = 'all'"
        >
          <AppIcon name="grid" /> Genel bakış <span>{{ todos.length }}</span>
        </button>
        <button
          :class="['nav-item', { selected: filter === 'active' }]"
          @click="filter = 'active'"
        >
          <AppIcon name="clock" /> Aktif görevler <span>{{ activeCount }}</span>
        </button>
        <button
          :class="['nav-item', { selected: filter === 'completed' }]"
          @click="filter = 'completed'"
        >
          <AppIcon name="check" /> Tamamlananlar
          <span>{{ completedCount }}</span>
        </button>
      </nav>
      <div class="sidebar-note">
        <span class="note-icon"><AppIcon name="leaf" :size="26" /></span>
        <h3>Küçük adımlar,<br />büyük değişimler.</h3>
        <p>Her tamamlanan görev,<br />kendine ayırdığın bir alan.</p>
        <div class="note-decoration" />
      </div>
      <div class="sidebar-bottom">
        <div class="profile">
          <span class="avatar">B</span>
          <div>
            <strong>Benim çalışma alanım</strong
            ><small>Bugün kendine odaklan.</small>
          </div>
        </div>
        <button class="logout-button" @click="logout">
          <AppIcon name="logout" :size="18" /> Çıkış yap
        </button>
      </div>
    </aside>
    <main class="main-content">
      <header class="topbar">
        <div class="breadcrumb">
          Çalışma alanım <span>/</span> <strong>Genel bakış</strong>
        </div>
        <span class="today"
          ><AppIcon name="calendar" :size="16" />{{ today }}</span
        >
      </header>
      <div class="page-content">
        <section class="page-heading">
          <div>
            <div class="eyebrow"><span /> DAHA AZ KARMAŞA, DAHA ÇOK ODAK</div>
            <h1>Güzel bir güne başla<span>.</span></h1>
            <p>Aklındakileri bir yere bırak. Önündeki adıma odaklan.</p>
          </div>
          <button class="button primary" @click="newTodo">
            <AppIcon name="plus" :size="18" /> Yeni görev
          </button>
        </section>
        <section class="overview" aria-label="Görev özeti">
          <div class="stat-card">
            <span class="stat-icon neutral"><AppIcon name="list" /></span>
            <div>
              <span class="stat-label">Toplam görev</span
              ><strong>{{ todos.length.toString().padStart(2, "0") }}</strong>
            </div>
            <span class="stat-caption">Hepsi bir arada</span>
          </div>
          <div class="stat-card">
            <span class="stat-icon amber"><AppIcon name="clock" /></span>
            <div>
              <span class="stat-label">Aktif görevler</span
              ><strong>{{ activeCount.toString().padStart(2, "0") }}</strong>
            </div>
            <span class="stat-caption">Sıradaki adımların</span>
          </div>
          <div class="stat-card">
            <span class="stat-icon green"><AppIcon name="check" /></span>
            <div>
              <span class="stat-label">Tamamlanan</span
              ><strong>{{ completedCount.toString().padStart(2, "0") }}</strong>
            </div>
            <span class="stat-caption">Emeğinin karşılığı</span>
          </div>
        </section>
        <div v-if="error" class="message error" role="alert">
          {{ error
          }}<button v-if="!loading" class="text-button" @click="loadTodos">
            Yeniden dene
          </button>
        </div>
        <div v-if="notice" class="message success" role="status">
          <AppIcon name="check" :size="17" />{{ notice
          }}<button
            class="icon-button"
            aria-label="Bildirimi kapat"
            @click="notice = ''"
          >
            <AppIcon name="close" :size="16" />
          </button>
        </div>
        <div class="content-grid">
          <section class="tasks-panel panel">
            <div class="panel-heading">
              <div>
                <h2>
                  Görevlerim <span class="count-badge">{{ todos.length }}</span>
                </h2>
                <p>Bir plan yap, bir adım at, fark yarat.</p>
              </div>
              <AppIcon name="list" :size="21" />
            </div>
            <div class="task-toolbar">
              <div class="filter-tabs" aria-label="Görev filtresi">
                <button
                  :class="{ active: filter === 'all' }"
                  @click="filter = 'all'"
                >
                  Tümü</button
                ><button
                  :class="{ active: filter === 'active' }"
                  @click="filter = 'active'"
                >
                  Aktif <span>{{ activeCount }}</span></button
                ><button
                  :class="{ active: filter === 'completed' }"
                  @click="filter = 'completed'"
                >
                  Tamamlanan
                </button>
              </div>
              <label class="search-box"
                ><AppIcon name="search" :size="17" /><input
                  v-model="search"
                  type="search"
                  placeholder="Görev ara..."
                  aria-label="Görev ara"
              /></label>
            </div>
            <div v-if="loading" class="empty-state" role="status">
              <span class="spinner" />
              <h3>Görevlerin yükleniyor…</h3>
            </div>
            <div v-else-if="!visibleTodos.length" class="empty-state">
              <span class="empty-icon"
                ><AppIcon :name="search ? 'search' : 'leaf'" :size="32"
              /></span>
              <h3>
                {{
                  search
                    ? "Bir eşleşme bulamadık"
                    : filter === "completed"
                      ? "Güzel işler sırada"
                      : filter === "active"
                        ? "Her şey tamam!"
                        : "Yeni bir başlangıca yer aç"
                }}
              </h3>
              <p>
                {{
                  search
                    ? "Başka bir kelimeyle aramayı deneyebilirsin."
                    : filter === "completed"
                      ? "Tamamladığın görevler burada yer alacak."
                      : filter === "active"
                        ? "Aktif görevin yok. Kendine biraz zaman ayır."
                        : "İlk görevini ekle, aklındakileri küçük adımlara dönüştür."
                }}
              </p>
              <button
                v-if="!search && filter !== 'completed'"
                class="button secondary"
                @click="newTodo"
              >
                <AppIcon name="plus" :size="17" /> Görev ekle
              </button>
            </div>
            <ul v-else class="task-list">
              <li
                v-for="todo in visibleTodos"
                :key="todo.id"
                :class="['task-item', { completed: !todo.isActive }]"
              >
                <button
                  :class="['task-checkbox', { checked: !todo.isActive }]"
                  :disabled="busyId !== null"
                  :aria-label="
                    todo.isActive
                      ? `${todo.title} görevini tamamla`
                      : `${todo.title} görevini aktifleştir`
                  "
                  :aria-pressed="!todo.isActive"
                  @click="toggleTodo(todo)"
                >
                  <AppIcon v-if="!todo.isActive" name="check" :size="15" />
                </button>
                <div class="task-details">
                  <h3>{{ todo.title }}</h3>
                  <p v-if="todo.description">{{ todo.description }}</p>
                  <div class="task-meta">
                    <span
                      :class="[
                        'status-badge',
                        todo.isActive ? 'status-active' : 'status-completed',
                      ]"
                      ><i />{{ todo.isActive ? "Aktif" : "Tamamlandı" }}</span
                    ><span
                      ><AppIcon name="calendar" :size="13" />{{
                        formatDate(todo.createDate)
                      }}</span
                    >
                  </div>
                </div>
                <div class="task-actions">
                  <button
                    class="icon-button"
                    :aria-label="`${todo.title} görevini düzenle`"
                    title="Düzenle"
                    @click="editTodo(todo)"
                  >
                    <AppIcon name="edit" :size="17" /></button
                  ><button
                    class="icon-button danger-icon"
                    :disabled="busyId !== null"
                    :aria-label="`${todo.title} görevini sil`"
                    title="Sil"
                    @click="deleteTarget = todo"
                  >
                    <AppIcon name="trash" :size="17" />
                  </button>
                </div>
              </li>
            </ul>
            <div class="list-footer">
              <span>{{ visibleTodos.length }} görev gösteriliyor</span
              ><span>Her adım önemli.<AppIcon name="leaf" :size="14" /></span>
            </div>
          </section>
          <aside class="right-column">
            <section class="panel form-panel">
              <div class="panel-heading">
                <div>
                  <h2>{{ id ? "Görevi düzenle" : "Aklında ne var?" }}</h2>
                  <p>
                    {{
                      id
                        ? "Küçük bir değişiklik, yeni bir adım."
                        : "Bir yere yaz, zihnin hafiflesin."
                    }}
                  </p>
                </div>
                <span class="form-plus"
                  ><AppIcon :name="id ? 'edit' : 'plus'" :size="19"
                /></span>
              </div>
              <form @submit.prevent="saveTodo">
                <label for="task-title">Görev başlığı <span>*</span></label
                ><input
                  id="task-title"
                  ref="titleInput"
                  v-model="title"
                  required
                  maxlength="200"
                  placeholder="Neyi başarmak istiyorsun?"
                /><label for="task-description"
                  >Açıklama <span class="optional">isteğe bağlı</span></label
                ><textarea
                  id="task-description"
                  v-model="description"
                  rows="4"
                  placeholder="Biraz detay ekle…"
                /><label class="switch-label"
                  ><span
                    ><strong>Aktif görev</strong
                    ><small>Yapılacaklar listende yer alsın.</small></span
                  ><input
                    v-model="isActive"
                    type="checkbox"
                    class="switch-input" /><span
                    class="switch"
                    aria-hidden="true" /></label
                ><button
                  class="button primary full-width"
                  :disabled="saving || !title.trim()"
                  type="submit"
                >
                  <AppIcon :name="id ? 'check' : 'plus'" :size="18" />{{
                    saving
                      ? "Kaydediliyor…"
                      : id
                        ? "Değişiklikleri kaydet"
                        : "Görev ekle"
                  }}</button
                ><button
                  v-if="id || title || description"
                  class="text-button reset-button"
                  type="button"
                  @click="clearForm"
                >
                  {{ id ? "Düzenlemeyi iptal et" : "Formu temizle" }}
                </button>
              </form>
            </section>
            <section class="progress-card">
              <div class="progress-heading">
                <span class="stat-icon green"
                  ><AppIcon name="sun" :size="20" /></span
                ><span>Adım adım, ileriye.</span>
              </div>
              <p>
                {{
                  todos.length
                    ? "Tamamladığın her görevle biraz daha yol alıyorsun."
                    : "Büyük planlar küçük bir görevle başlar."
                }}
              </p>
              <div class="progress-label">
                <span>Tamamlanma oranı</span><strong>%{{ progress }}</strong>
              </div>
              <div
                class="progress-track"
                role="progressbar"
                :aria-valuenow="progress"
                aria-valuemin="0"
                aria-valuemax="100"
                aria-label="Görev tamamlanma oranı"
              >
                <span :style="{ width: `${progress}%` }" />
              </div>
              <small
                >{{ completedCount }} / {{ todos.length }} görev
                tamamlandı</small
              >
            </section>
          </aside>
        </div>
        <footer class="page-footer">
          <span>Biraz plan, biraz odak. Gerisi sende.</span
          ><span
            >odak. ile daha sakin bir gün <AppIcon name="leaf" :size="13"
          /></span>
        </footer>
      </div>
    </main>
    <dialog
      ref="deleteDialog"
      class="delete-dialog"
      role="alertdialog"
      aria-labelledby="delete-heading"
      aria-describedby="delete-description"
      @click.self="busyId === null && (deleteTarget = null)"
      @cancel.prevent="busyId === null && (deleteTarget = null)"
    >
      <div v-if="deleteTarget">
        <span class="delete-symbol"><AppIcon name="trash" :size="24" /></span>
        <h2 id="delete-heading">Görev silinsin mi?</h2>
        <p id="delete-description">
          “{{ deleteTarget.title }}” kalıcı olarak silinecek.
        </p>
        <div class="dialog-actions">
          <button
            autofocus
            class="button secondary"
            :disabled="busyId !== null"
            @click="deleteTarget = null"
          >
            Vazgeç</button
          ><button
            class="button danger"
            :disabled="busyId !== null"
            @click="deleteTodo"
          >
            {{ busyId !== null ? "Siliniyor…" : "Görevi sil" }}
          </button>
        </div>
      </div>
    </dialog>
  </div>
</template>
