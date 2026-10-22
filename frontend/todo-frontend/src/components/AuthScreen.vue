<script setup lang="ts">
import type { MessageKey } from "@/i18n/messages";
import { useI18n } from "@/i18n";
import { ref } from "vue";
import { useRouter } from "vue-router";
import api from "@/api/api";
import AppIcon from "@/components/AppIcon.vue";
import LanguageToggle from "@/components/LanguageToggle.vue";
import type { LoginRequest, RegisterRequest } from "@/models/auth";

const { t } = useI18n();
const props = defineProps<{ mode: "login" | "register" }>();
const router = useRouter();
const fullName = ref("");
const email = ref("");
const password = ref("");
const confirmPassword = ref("");
const showPassword = ref(false);
const showConfirmPassword = ref(false);
const busy = ref(false);
const error = ref<MessageKey | "">("");
async function submit() {
  if (busy.value) return;
  error.value = "";
  if (props.mode === "register" && password.value !== confirmPassword.value) {
    error.value = "passwordMismatch";
    return;
  }
  busy.value = true;
  try {
    const request: LoginRequest | RegisterRequest =
      props.mode === "register"
        ? {
            fullName: fullName.value.trim(),
            email: email.value.trim(),
            password: password.value,
            confirmPassword: confirmPassword.value,
          }
        : { email: email.value.trim(), password: password.value };
    const response = await api.post(`/Auth/${props.mode}`, request);
    localStorage.setItem("token", response.data.token);
    await router.push("/todos");
  } catch {
    error.value =
      props.mode === "login"
        ? "loginError"
        : "registerError";
  } finally {
    busy.value = false;
  }
}
</script>

<template>
  <main class="auth-page">
    <section class="auth-story">
      <RouterLink :to="mode === 'login' ? '/login' : '/register'" class="brand"
        ><span class="brand-icon"><AppIcon name="route" :size="23" /></span>
        rota<span class="brand-dot">↗</span></RouterLink
      >
      <div class="auth-story-content">
        <div class="eyebrow"><span /> {{ t('authEyebrow') }}</div>
        <h1>{{ t('authHeadline') }}<br /><em>{{ t('authHeadlineEm') }}</em></h1>
        <p>
          {{ t('authIntro') }}<br />{{ t('authIntroEnd') }}
        </p>
        <div class="route-poster" aria-hidden="true">
          <span class="poster-label">{{ t('posterLabel') }}</span
          ><svg viewBox="0 0 400 300">
            <path
              d="M55 270V210Q55 165 100 165H265Q320 165 320 110V85Q320 35 270 35H200"
            />
            <circle cx="55" cy="270" r="10" />
            <circle cx="200" cy="35" r="10" />
          </svg>
          <div class="poster-note">
            <AppIcon name="check" :size="18" /> {{ t('posterNote') }}
          </div>
          <span class="poster-compass"><AppIcon name="route" :size="46" /></span
          ><span class="poster-caption">{{ t('posterCaption') }}</span>
        </div>
        <div class="auth-benefits">
          <span><AppIcon name="check" :size="15" /> {{ t('benefitSimple') }}</span
          ><span><AppIcon name="check" :size="15" /> {{ t('benefitPersonal') }}</span>
        </div>
      </div>
      <span class="auth-story-footer">{{ t('authStoryFooter') }}</span>
    </section>
    <section class="auth-form-area">
      <div class="auth-top-link">
        <LanguageToggle />
        <span>{{
          mode === "login" ? t("noAccount") : t("hasAccount")
        }}</span
        ><RouterLink :to="mode === 'login' ? '/register' : '/login'"
          >{{ mode === "login" ? t("createAccount") : t("signIn") }}
          <AppIcon name="arrow" :size="15"
        /></RouterLink>
      </div>
      <div class="auth-form-content" :key="mode">
        <span class="auth-welcome-icon"
          ><AppIcon :name="mode === 'login' ? 'sun' : 'route'" :size="27"
        /></span>
        <div class="eyebrow">
          {{
            mode === "login" ? t("loginEyebrow") : t("registerEyebrow")
          }}
        </div>
        <h2>
          {{ mode === "login" ? t("welcomeBack") : t("registerHeading") }}
        </h2>
        <p>
          {{
            mode === "login"
              ? t("loginSubtitle")
              : t("registerSubtitle")
          }}
        </p>
        <form v-localized-validation @submit.prevent="submit">
          <div v-if="error" class="message error" role="alert">{{ t(error) }}</div>
          <template v-if="mode === 'register'"
            ><label for="full-name">{{ t('fullName') }}</label
            ><input
              id="full-name"
              v-model="fullName"
              autocomplete="name"
              :placeholder="t('fullNamePlaceholder')"
              required
              pattern=".*\S.*" /></template
          ><label for="email">{{ t('email') }}</label
          ><input
            id="email"
            v-model="email"
            type="email"
            autocomplete="email"
            :placeholder="t('emailPlaceholder')"
            required
          /><label for="password">{{ t('password') }}</label>
          <div class="password-field">
            <input
              id="password"
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              :autocomplete="
                mode === 'login' ? 'current-password' : 'new-password'
              "
              :placeholder="t('passwordPlaceholder')"
              required
            /><button
              type="button"
              class="icon-button"
              :aria-label="showPassword ? t('hidePassword') : t('showPassword')"
              :aria-pressed="showPassword"
              @click="showPassword = !showPassword"
            >
              <AppIcon name="eye" :size="19" />
            </button>
          </div>
          <template v-if="mode === 'register'">
            <label for="confirm-password">{{ t('confirmPassword') }}</label>
            <div class="password-field">
              <input
                id="confirm-password"
                v-model="confirmPassword"
                :type="showConfirmPassword ? 'text' : 'password'"
                autocomplete="new-password"
                :placeholder="t('confirmPlaceholder')"
                required
              />
              <button
                type="button"
                class="icon-button"
                :aria-label="
                  showConfirmPassword
                    ? t('hideConfirmation')
                    : t('showConfirmation')
                "
                :aria-pressed="showConfirmPassword"
                @click="showConfirmPassword = !showConfirmPassword"
              >
                <AppIcon name="eye" :size="19" />
              </button>
            </div>
          </template>
          <button
            class="button primary full-width auth-submit"
            type="submit"
            :disabled="busy"
          >
            {{
              busy
                ? t("pleaseWait")
                : mode === "login"
                  ? t("signIn")
                  : t("createMyAccount")
            }}<AppIcon name="arrow" :size="18" />
          </button>
        </form>
        <div class="auth-form-note">
          <AppIcon name="route" :size="15" /><span
            >{{ t('authNote') }}</span
          >
        </div>
      </div>
      <footer class="auth-footer">{{ t('authFooter') }}</footer>
    </section>
  </main>
</template>
