<script setup lang="ts">
import type { MessageKey } from "@/i18n/messages";
import { useI18n } from "@/i18n";
import api from "@/api/api";
import AppIcon from "@/components/AppIcon.vue";
import LanguageToggle from "@/components/LanguageToggle.vue";
import type { CreateTodoRequest, Todo, UpdateTodoRequest } from "@/models/todo";
import { computed, nextTick, onMounted, ref, watch } from "vue";
import { useRouter } from "vue-router";

const { t, locale, formatPercent } = useI18n();
const router = useRouter();
const todos = ref<Todo[]>([]);
const filter = ref<"all" | "active" | "inactive" | "completed" | "incomplete">("all");
const search = ref("");
const loading = ref(true);
const saving = ref(false);
const busyId = ref<number | null>(null);
const error = ref<MessageKey | "">("");
const notice = ref<MessageKey | "">("");
const id = ref(0);
const title = ref("");
const description = ref("");
const isActive = ref(true);
const isCompleted = ref(false);
const createDate = ref<Date>(new Date());
const editorDialog = ref<HTMLDialogElement | null>(null);
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
const completedCount = computed(() => todos.value.filter((todo) => todo.isCompleted).length);
const filterLabels = { all: "overview", active: "activeTasks", inactive: "inactiveTasks", completed: "completedTasks", incomplete: "incompleteTasks" } as const;
const progress = computed(() =>
  todos.value.length
    ? Math.round((completedCount.value / todos.value.length) * 100)
    : 0,
);
const visibleTodos = computed(() =>
  todos.value.filter(
    (todo) =>
      (filter.value === "all" ||
        (filter.value === "active" && todo.isActive) ||
        (filter.value === "inactive" && !todo.isActive) ||
        (filter.value === "completed" && todo.isCompleted) ||
        (filter.value === "incomplete" && !todo.isCompleted)) &&
      `${todo.title} ${todo.description}`
        .toLocaleLowerCase(locale.value)
        .includes(search.value.toLocaleLowerCase(locale.value)),
  ),
);
const today = computed(() => new Intl.DateTimeFormat(locale.value, {
  day: "numeric",
  month: "long",
  year: "numeric",
  weekday: "long",
}).format(new Date()));
function formatDate(value: Date) {
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? t("noDate")
    : new Intl.DateTimeFormat(locale.value, {
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
      "loadError";
  } finally {
    loading.value = false;
  }
}
function clearForm() {
  id.value = 0;
  title.value = "";
  description.value = "";
  isActive.value = true;
  isCompleted.value = false;
}
async function focusForm() {
  editorDialog.value?.showModal();
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
  isCompleted.value = todo.isCompleted;
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
      isCompleted: isCompleted.value,
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
      ? "taskUpdated"
      : "taskCreated";
    clearForm();
    editorDialog.value?.close();
    await loadTodos();
  } catch {
    error.value = "saveError";
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
      isActive: todo.isActive,
      isCompleted: !todo.isCompleted,
      createDate: todo.createDate,
      updateDate: new Date(),
    } satisfies UpdateTodoRequest);
    todo.isCompleted = !todo.isCompleted;
  } catch {
    error.value = "toggleError";
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
    notice.value = "taskDeleted";
  } catch {
    deleteTarget.value = null;
    error.value = "deleteError";
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
    <header class="sidebar">
      <RouterLink to="/todos" class="brand"
        ><span class="brand-icon"><AppIcon name="route" :size="23" /></span>
        rota<span class="brand-dot">↗</span></RouterLink
      >
      <nav :aria-label="t('taskMenu')">
        <button
          :class="['nav-item', { selected: filter === 'all' }]"
          :aria-pressed="filter === 'all'"
          @click="filter = 'all'"
        >
          <AppIcon name="grid" /> {{ t('overview') }} <span>{{ todos.length }}</span>
        </button>
        <button
          :class="['nav-item', { selected: filter === 'active' }]"
          :aria-pressed="filter === 'active'"
          @click="filter = 'active'"
        >
          <AppIcon name="clock" /> {{ t('activeTasks') }} <span>{{ activeCount }}</span>
        </button>
        <button
          :class="['nav-item', { selected: filter === 'completed' }]"
          :aria-pressed="filter === 'completed'"
          @click="filter = 'completed'"
        >
          <AppIcon name="check" /> {{ t('completedTasks') }}
          <span>{{ completedCount }}</span>
        </button>
      </nav>
      <div class="sidebar-bottom">
        <LanguageToggle />
        <button class="logout-button" @click="logout">
          <AppIcon name="logout" :size="18" /> {{ t('signOut') }}
        </button>
      </div>
    </header>
    <main class="main-content">
      <header class="topbar">
        <div class="breadcrumb">
          {{ t('workspace') }} <span>/</span>
          <strong>{{
            t(filterLabels[filter])
          }}</strong>
        </div>
        <span class="today"
          ><AppIcon name="calendar" :size="16" />{{ today }}</span
        >
      </header>
      <div class="page-content">
        <section class="page-heading">
          <div>
            <div class="eyebrow"><span /> {{ t('pageEyebrow') }}</div>
            <h1>{{ t('pageHeading') }}<span>.</span></h1>
            <p>{{ t('pageSubtitle') }}</p>
          </div>
          <button class="button primary" @click="newTodo">
            <AppIcon name="plus" :size="18" /> {{ t('newTask') }}
          </button>
        </section>
        <section class="focus-banner" :aria-label="t('progressSummary')">
          <div>
            <span class="banner-kicker">{{ t('bannerKicker') }}</span>
            <h2>{{ t('bannerHeading') }}<br /><em>{{ t('bannerHeadingEm') }}</em></h2>
            <p>
              {{
                todos.length
                  ? t('progressMessage', { total: todos.length, completed: completedCount })
                  : t("firstTaskPrompt")
              }}
            </p>
          </div>
          <div class="route-art" aria-hidden="true">
            <svg viewBox="0 0 420 180">
              <path
                d="M10 145 H105 Q145 145 145 105 V75 Q145 35 185 35 H240 Q280 35 280 75 V105 Q280 145 320 145 H400"
              />
              <circle cx="12" cy="145" r="8" />
              <circle cx="210" cy="35" r="8" />
              <circle cx="400" cy="145" r="8" /></svg
            ><span class="route-marker"
              ><AppIcon name="route" :size="28" /></span
            ><span class="route-caption"
              >{{ t('routeCaption') }}</span
            >
          </div>
          <div class="journey-total">
            <strong>{{ formatPercent(progress / 100) }}</strong
            ><span>{{ t('progressCaption') }}</span>
          </div>
        </section>
        <section class="overview" :aria-label="t('taskSummary')">
          <div class="stat-card">
            <span class="stat-icon neutral"><AppIcon name="list" /></span>
            <div>
              <span class="stat-label">{{ t('totalTasks') }}</span
              ><strong>{{ todos.length.toString().padStart(2, "0") }}</strong>
            </div>
            <span class="stat-caption">{{ t('allTogether') }}</span>
          </div>
          <div class="stat-card">
            <span class="stat-icon amber"><AppIcon name="clock" /></span>
            <div>
              <span class="stat-label">{{ t('activeTasks') }}</span
              ><strong>{{ activeCount.toString().padStart(2, "0") }}</strong>
            </div>
            <span class="stat-caption">{{ t('nextSteps') }}</span>
          </div>
          <div class="stat-card">
            <span class="stat-icon green"><AppIcon name="check" /></span>
            <div>
              <span class="stat-label">{{ t('completed') }}</span
              ><strong>{{ completedCount.toString().padStart(2, "0") }}</strong>
            </div>
            <span class="stat-caption">{{ t('effortReward') }}</span>
          </div>
        </section>
        <div v-if="error" class="message error" role="alert">
          {{ t(error) }}<button v-if="!loading" class="text-button" @click="loadTodos">
            {{ t('retryLoad') }}
          </button>
        </div>
        <div v-if="notice" class="message success" role="status">
          <AppIcon name="check" :size="17" />{{ t(notice) }}<button
            class="icon-button"
            :aria-label="t('dismissNotice')"
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
                  {{ t('tasksHeading') }}
                  <span class="count-badge">{{ todos.length }}</span>
                </h2>
                <p>{{ t('tasksSubtitle') }}</p>
              </div>
              <AppIcon name="list" :size="21" />
            </div>
            <div class="task-toolbar">
              <div class="filter-tabs" :aria-label="t('taskFilter')">
                <button
                  :class="{ active: filter === 'all' }"
                  :aria-pressed="filter === 'all'"
                  @click="filter = 'all'"
                >
                  {{ t('all') }}</button
                ><button
                  :class="{ active: filter === 'active' }"
                  :aria-pressed="filter === 'active'"
                  @click="filter = 'active'"
                >
                  {{ t('active') }} <span>{{ activeCount }}</span></button
                ><button
                  :class="{ active: filter === 'completed' }"
                  :aria-pressed="filter === 'completed'"
                  @click="filter = 'completed'"
                >
                  {{ t('completed') }}
                </button>
                <button :class="{ active: filter === 'incomplete' }" :aria-pressed="filter === 'incomplete'" @click="filter = 'incomplete'">{{ t('incomplete') }}</button>
                <button :class="{ active: filter === 'inactive' }" :aria-pressed="filter === 'inactive'" @click="filter = 'inactive'">{{ t('inactive') }}</button>
              </div>
              <label class="search-box"
                ><AppIcon name="search" :size="17" /><input
                  v-model="search"
                  type="search"
                  :placeholder="t('searchPlaceholder')"
                  :aria-label="t('searchTasks')"
              /></label>
            </div>
            <div v-if="loading" class="empty-state" role="status">
              <span class="spinner" />
              <h3>{{ t('loadingTasks') }}</h3>
            </div>
            <div v-else-if="!visibleTodos.length" class="empty-state">
              <span class="empty-icon"
                ><AppIcon :name="search ? 'search' : 'route'" :size="32"
              /></span>
              <h3>
                {{
                  search
                    ? t("emptySearch")
                    : filter === "completed"
                      ? t("emptyCompleted")
                      : filter === "active"
                        ? t("emptyActive")
                        : filter === "inactive" ? t("emptyInactive")
                        : filter === "incomplete" ? t("emptyIncomplete")
                        : t("emptyAll")
                }}
              </h3>
              <p>
                {{
                  search
                    ? t("emptySearchHelp")
                    : filter === "completed"
                      ? t("emptyCompletedHelp")
                      : filter === "active"
                        ? t("emptyActiveHelp")
                        : filter === "inactive" ? t("emptyInactiveHelp")
                        : filter === "incomplete" ? t("emptyIncompleteHelp")
                        : t("emptyAllHelp")
                }}
              </p>
              <button
                v-if="!search && filter !== 'completed'"
                class="button secondary"
                @click="newTodo"
              >
                <AppIcon name="plus" :size="17" /> {{ t('addTask') }}
              </button>
            </div>
            <ul v-else class="task-list">
              <li
                v-for="todo in visibleTodos"
                :key="todo.id"
                :class="['task-item', { completed: todo.isCompleted }]"
              >
                <button
                  :class="['task-checkbox', { checked: todo.isCompleted }]"
                  :disabled="busyId !== null"
                  :aria-label="
                    !todo.isCompleted
                      ? t('completeTaskLabel', { title: todo.title })
                      : t('incompleteTaskLabel', { title: todo.title })
                  "
                  :aria-pressed="todo.isCompleted"
                  @click="toggleTodo(todo)"
                >
                  <AppIcon v-if="todo.isCompleted" name="check" :size="15" />
                </button>
                <div class="task-details">
                  <h3>{{ todo.title }}</h3>
                  <p v-if="todo.description">{{ todo.description }}</p>
                  <div class="task-meta">
                    <span
                      :class="[
                        'status-badge',
                        todo.isActive ? 'status-active' : 'status-inactive',
                      ]"
                      ><i />{{ todo.isActive ? t("active") : t("inactive") }}</span
                    ><span :class="['status-badge', todo.isCompleted ? 'status-completed' : 'status-active']">
                      <i />{{ todo.isCompleted ? t("done") : t("notDone") }}
                    </span><span
                      ><AppIcon name="calendar" :size="13" />{{
                        formatDate(todo.createDate)
                      }}</span
                    >
                  </div>
                </div>
                <div class="task-actions">
                  <button
                    class="icon-button"
                    :aria-label="t('editTaskLabel', { title: todo.title })"
                    :title="t('edit')"
                    @click="editTodo(todo)"
                  >
                    <AppIcon name="edit" :size="17" /></button
                  ><button
                    class="icon-button danger-icon"
                    :disabled="busyId !== null"
                    :aria-label="t('deleteTaskLabel', { title: todo.title })"
                    :title="t('delete')"
                    @click="deleteTarget = todo"
                  >
                    <AppIcon name="trash" :size="17" />
                  </button>
                </div>
              </li>
            </ul>
            <div class="list-footer">
              <span>{{ t('tasksShown', { count: visibleTodos.length }) }}</span
              ><span>{{ t('everyStep') }}<AppIcon name="route" :size="14" /></span>
            </div>
          </section>
          <dialog
            ref="editorDialog"
            class="editor-dialog"
            aria-labelledby="editor-title"
            @click.self="!saving && editorDialog?.close()"
            @cancel="saving && $event.preventDefault()"
          >
            <section class="panel form-panel">
              <div class="panel-heading">
                <div>
                  <h2 id="editor-title">
                    {{ id ? t("editTask") : t("addTaskHeading") }}
                  </h2>
                  <p>
                    {{
                      id
                        ? t("editTaskHelp")
                        : t("addTaskHelp")
                    }}
                  </p>
                </div>
                <button
                  class="icon-button"
                  :aria-label="t('closeForm')"
                  :disabled="saving"
                  @click="editorDialog?.close()"
                >
                  <AppIcon name="close" />
                </button>
              </div>
              <form v-localized-validation @submit.prevent="saveTodo">
                <div v-if="error" class="message error" role="alert">
                  {{ t(error) }}
                </div>
                <label for="task-title">{{ t('taskTitle') }} <span>*</span></label
                ><input
                  id="task-title"
                  ref="titleInput"
                  v-model="title"
                  required
                  maxlength="200"
                  :placeholder="t('titlePlaceholder')"
                /><label for="task-description"
                  >{{ t('description') }} <span class="optional">{{ t('optional') }}</span></label
                ><textarea
                  id="task-description"
                  v-model="description"
                  rows="4"
                  :placeholder="t('descriptionPlaceholder')"
                /><label class="switch-label"
                  ><span
                    ><strong>{{ t('activeTask') }}</strong
                    ><small>{{ t('activeHelp') }}</small></span
                  ><input
                    v-model="isActive"
                    type="checkbox"
                    class="switch-input" /><span
                    class="switch"
                    aria-hidden="true" /></label
                ><label class="switch-label">
                  <span><strong>{{ t('done') }}</strong><small>{{ t('completedHelp') }}</small></span>
                  <input v-model="isCompleted" type="checkbox" class="switch-input" />
                  <span class="switch" aria-hidden="true" />
                </label><button
                  class="button primary full-width"
                  :disabled="saving || !title.trim()"
                  type="submit"
                >
                  <AppIcon :name="id ? 'check' : 'plus'" :size="18" />{{
                    saving
                      ? t("saving")
                      : id
                        ? t("saveChanges")
                        : t("addTask")
                  }}</button
                ><button
                  v-if="id || title || description"
                  class="text-button reset-button"
                  type="button"
                  @click="clearForm"
                >
                  {{ id ? t("cancelEditing") : t("clearForm") }}
                </button>
              </form>
            </section>
          </dialog>
        </div>
        <footer class="page-footer">
          <span>{{ t('footerPlan') }}</span
          ><span
            >{{ t('footerBrand') }} <AppIcon name="route" :size="13"
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
        <h2 id="delete-heading">{{ t('deleteHeading') }}</h2>
        <p id="delete-description">
          {{ t('deleteDescription', { title: deleteTarget.title }) }}
        </p>
        <div class="dialog-actions">
          <button
            autofocus
            class="button secondary"
            :disabled="busyId !== null"
            @click="deleteTarget = null"
          >
            {{ t('cancel') }}</button
          ><button
            class="button danger"
            :disabled="busyId !== null"
            @click="deleteTodo"
          >
            {{ busyId !== null ? t("deleting") : t("deleteTask") }}
          </button>
        </div>
      </div>
    </dialog>
  </div>
</template>
