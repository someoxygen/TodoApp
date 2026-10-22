import type { ObjectDirective } from "vue";
import { watchEffect } from "vue";
import { useI18n } from "./index";

// Browser validation messages otherwise follow the browser's language, not the app's.
export const localizedValidation: ObjectDirective<HTMLFormElement> = {
  mounted(form) {
    const { t } = useI18n();
    const update = () => {
      for (const element of Array.from(form.elements)) {
        if (!(element instanceof HTMLInputElement || element instanceof HTMLTextAreaElement)) continue;
        element.setCustomValidity("");
        if (element.validity.valueMissing || (element.required && !element.value.trim())) {
          element.setCustomValidity(t("requiredField"));
        } else if (element.validity.typeMismatch) {
          element.setCustomValidity(t("invalidEmail"));
        } else if (element.validity.tooLong) {
          element.setCustomValidity(t("tooLong"));
        } else if (!element.validity.valid) {
          element.setCustomValidity(t("invalidField"));
        }
      }
    };
    const stop = watchEffect(update);
    form.addEventListener("input", update);
    form.addEventListener("invalid", update, true);
    // Covers fields mounted conditionally and programmatic changes to v-model.
    const observer = new MutationObserver(update);
    observer.observe(form, { childList: true, subtree: true });
    cleanups.set(form, () => {
      stop();
      observer.disconnect();
      form.removeEventListener("input", update);
      form.removeEventListener("invalid", update, true);
    });
  },
  updated(form) {
    form.dispatchEvent(new Event("input"));
  },
  unmounted(form) {
    cleanups.get(form)?.();
    cleanups.delete(form);
  },
};

const cleanups = new WeakMap<HTMLFormElement, () => void>();
