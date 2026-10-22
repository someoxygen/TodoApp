<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import api from "@/api/api";
import AppIcon from "@/components/AppIcon.vue";
import type { LoginRequest, RegisterRequest } from "@/models/auth";
const props = defineProps<{ mode: "login" | "register" }>();
const router = useRouter();
const fullName = ref("");
const email = ref("");
const password = ref("");
const confirmPassword = ref("");
const showPassword = ref(false);
const showConfirmPassword = ref(false);
const busy = ref(false);
const error = ref("");
async function submit() {
  if (busy.value) return;
  error.value = "";
  if (props.mode === "register" && password.value !== confirmPassword.value) {
    error.value = "Şifreler eşleşmiyor. Lütfen tekrar kontrol et.";
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
        ? "Giriş yapılamadı. E-posta ve şifreni kontrol edip tekrar dene."
        : "Hesap oluşturulamadı. Bilgilerini kontrol edip tekrar dene.";
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
        <div class="eyebrow"><span /> DAHA FERAH BİR GÜN İÇİN</div>
        <h1>Planın olsun.<br /><em>Yolun açık olsun.</em></h1>
        <p>
          Zihnindeki kalabalığı geride bırak.<br />Yapmak istediklerine kendi
          rotanı çiz.
        </p>
        <div class="route-poster" aria-hidden="true">
          <span class="poster-label">BİR PLAN, BİNLERCE OLASILIK.</span
          ><svg viewBox="0 0 400 300">
            <path
              d="M55 270V210Q55 165 100 165H265Q320 165 320 110V85Q320 35 270 35H200"
            />
            <circle cx="55" cy="270" r="10" />
            <circle cx="200" cy="35" r="10" />
          </svg>
          <div class="poster-note">
            <AppIcon name="check" :size="18" /> Bir iş daha tamam.
          </div>
          <span class="poster-compass"><AppIcon name="route" :size="46" /></span
          ><span class="poster-caption">Yönünü sen belirle.</span>
        </div>
        <div class="auth-benefits">
          <span><AppIcon name="check" :size="15" /> Sade ve düzenli</span
          ><span><AppIcon name="check" :size="15" /> Tamamen sana ait</span>
        </div>
      </div>
      <span class="auth-story-footer">ROTA / KİŞİSEL PLANLAMA ALANIN</span>
    </section>
    <section class="auth-form-area">
      <div class="auth-top-link">
        <span>{{
          mode === "login" ? "Henüz hesabın yok mu?" : "Zaten hesabın var mı?"
        }}</span
        ><RouterLink :to="mode === 'login' ? '/register' : '/login'"
          >{{ mode === "login" ? "Hesap oluştur" : "Giriş yap" }}
          <AppIcon name="arrow" :size="15"
        /></RouterLink>
      </div>
      <div class="auth-form-content" :key="mode">
        <span class="auth-welcome-icon"
          ><AppIcon :name="mode === 'login' ? 'sun' : 'route'" :size="27"
        /></span>
        <div class="eyebrow">
          {{
            mode === "login" ? "KALDIĞIN YERDEN DEVAM ET" : "YENİ BİR BAŞLANGIÇ"
          }}
        </div>
        <h2>
          {{ mode === "login" ? "Yeniden merhaba." : "Kendi rotanı oluştur." }}
        </h2>
        <p>
          {{
            mode === "login"
              ? "Planların ve küçük başarıların seni bekliyor."
              : "Daha düzenli günler için ilk adımını at."
          }}
        </p>
        <form @submit.prevent="submit">
          <div v-if="error" class="message error" role="alert">{{ error }}</div>
          <template v-if="mode === 'register'"
            ><label for="full-name">Ad soyad</label
            ><input
              id="full-name"
              v-model="fullName"
              autocomplete="name"
              placeholder="Adın ve soyadın"
              required
              pattern=".*\S.*" /></template
          ><label for="email">E-posta adresi</label
          ><input
            id="email"
            v-model="email"
            type="email"
            autocomplete="email"
            placeholder="ornek@eposta.com"
            required
          /><label for="password">Şifre</label>
          <div class="password-field">
            <input
              id="password"
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              :autocomplete="
                mode === 'login' ? 'current-password' : 'new-password'
              "
              placeholder="Şifreni gir"
              required
            /><button
              type="button"
              class="icon-button"
              :aria-label="showPassword ? 'Şifreyi gizle' : 'Şifreyi göster'"
              :aria-pressed="showPassword"
              @click="showPassword = !showPassword"
            >
              <AppIcon name="eye" :size="19" />
            </button>
          </div>
          <template v-if="mode === 'register'">
            <label for="confirm-password">Şifre tekrarı</label>
            <div class="password-field">
              <input
                id="confirm-password"
                v-model="confirmPassword"
                :type="showConfirmPassword ? 'text' : 'password'"
                autocomplete="new-password"
                placeholder="Şifreni tekrar gir"
                required
              />
              <button
                type="button"
                class="icon-button"
                :aria-label="
                  showConfirmPassword
                    ? 'Şifre tekrarını gizle'
                    : 'Şifre tekrarını göster'
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
                ? "Lütfen bekle…"
                : mode === "login"
                  ? "Giriş yap"
                  : "Hesabımı oluştur"
            }}<AppIcon name="arrow" :size="18" />
          </button>
        </form>
        <div class="auth-form-note">
          <AppIcon name="route" :size="15" /><span
            >Her şeyi aynı anda yapmak zorunda değilsin.</span
          >
        </div>
      </div>
      <footer class="auth-footer">AZ KARMAŞA. DAHA ÇOK HAYAT.</footer>
    </section>
  </main>
</template>
