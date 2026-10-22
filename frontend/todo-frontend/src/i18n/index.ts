import { computed, readonly, ref, watchEffect } from "vue";
import { messages, type MessageKey } from "./messages.ts";

export type Language = "tr" | "en";
const storageKey = "rota-language";

function savedLanguage(): Language {
  try {
    return localStorage.getItem(storageKey) === "en" ? "en" : "tr";
  } catch {
    return "tr";
  }
}

const language = ref<Language>(savedLanguage());
const locale = computed(() => language.value === "tr" ? "tr-TR" : "en-US");

function t(key: MessageKey | "", params: Record<string, string | number> = {}): string {
  if (!key) return "";
  return messages[key][language.value].replace(/\{(\w+)\}/g, (match, name: string) =>
    params[name] === undefined ? match : String(params[name]),
  );
}

function setLanguage(value: Language) {
  language.value = value;
  try {
    localStorage.setItem(storageKey, value);
  } catch {
    // Language switching still works when browser storage is unavailable.
  }
}

function formatPercent(value: number) {
  return new Intl.NumberFormat(locale.value, { style: "percent" }).format(value);
}

export function useI18n() {
  return { language: readonly(language), locale, t, setLanguage, formatPercent };
}

export function initializeI18n() {
  watchEffect(() => {
    document.documentElement.lang = language.value;
    document.title = t("documentTitle");
    document.querySelector('meta[name="description"]')?.setAttribute("content", t("documentDescription"));
  });
}
