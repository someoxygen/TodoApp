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
const showPassword = ref(false);
const busy = ref(false);
const error = ref("");
async function submit() {
  if (busy.value) return;
  error.value = "";
  busy.value = true;
  try {
    const request: LoginRequest | RegisterRequest =
      props.mode === "register"
        ? {
            fullName: fullName.value.trim(),
            email: email.value.trim(),
            password: password.value,
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
        ><span class="brand-icon"><AppIcon name="check" :size="23" /></span>
        odak<span class="brand-dot">.</span></RouterLink
      >
      <div class="auth-story-content">
        <div class="eyebrow"><span /> GÜNÜNE BİRAZ ALAN AÇ</div>
        <h1>Daha sade bir plan.<br />Daha hafif bir zihin<span>.</span></h1>
        <p>
          Aklındaki işleri küçük adımlara dönüştür.<br />Senin için önemli olana
          yer kalsın.
        </p>
        <div class="auth-illustration" aria-hidden="true">
          <div class="illustration-top">
            <span class="mini-dots"><i /><i /><i /></span
            ><span>Bugünün küçük adımları</span
            ><AppIcon name="sun" :size="18" />
          </div>
          <div class="illustration-task">
            <span class="mini-check"><AppIcon name="check" :size="14" /></span
            ><span class="struck">Güne bir planla başla</span
            ><span class="mini-tag">Tamamlandı</span>
          </div>
          <div class="illustration-task">
            <span class="mini-check"><AppIcon name="check" :size="14" /></span
            ><span class="struck">Kendine zaman ayır</span>
          </div>
          <div class="illustration-task">
            <span class="mini-empty" /><span>Yeni fikirlere yer aç</span
            ><span class="mini-tag pending">Sıradaki</span>
          </div>
          <div class="illustration-footer">
            <span>Her küçük adım bir başlangıç.</span
            ><AppIcon name="leaf" :size="17" />
          </div>
          <span class="floating-leaf"><AppIcon name="leaf" :size="30" /></span>
        </div>
        <div class="auth-benefits">
          <span><AppIcon name="check" :size="15" /> Sade ve düzenli</span
          ><span><AppIcon name="check" :size="15" /> Tamamen sana ait</span>
        </div>
      </div>
      <span class="auth-story-footer"
        >Biraz plan, biraz odak. Gerisi sende.</span
      >
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
          ><AppIcon :name="mode === 'login' ? 'sun' : 'leaf'" :size="27"
        /></span>
        <div class="eyebrow">
          {{
            mode === "login" ? "KALDIĞIN YERDEN DEVAM ET" : "YENİ BİR BAŞLANGIÇ"
          }}
        </div>
        <h2>
          {{ mode === "login" ? "Tekrar hoş geldin." : "Birlikte başlayalım." }}
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
          <AppIcon name="leaf" :size="15" /><span
            >Her şeyi aynı anda yapmak zorunda değilsin.</span
          >
        </div>
      </div>
      <footer class="auth-footer">DAHA AZ KARMAŞA. DAHA ÇOK ODAK.</footer>
    </section>
  </main>
</template>
