import { useEffect, type RefObject } from "react";

export function useSearchShortcut(
  searchInputRef: RefObject<HTMLInputElement | null>,
  onClear: () => void
) {
  useEffect(() => {
    function handleKeyDown(e: KeyboardEvent) {
      const activeEl = document.activeElement;
      const target = (e.target as HTMLElement | null) ?? (activeEl as HTMLElement | null);
      const isSearchActive = activeEl === searchInputRef.current;

      if (e.key === "/" && !isSearchActive) {
        const isEditable =
          target instanceof HTMLInputElement ||
          target instanceof HTMLTextAreaElement ||
          target instanceof HTMLSelectElement ||
          Boolean(target?.isContentEditable) ||
          target?.getAttribute("role") === "textbox" ||
          Boolean(target?.closest('input, textarea, select, [contenteditable="true"], [role="textbox"]'));

        if (!isEditable && !e.ctrlKey && !e.metaKey && !e.altKey) {
          e.preventDefault();
          searchInputRef.current?.focus();
        }
      } else if (e.key === "Escape" && isSearchActive) {
        onClear();
        searchInputRef.current?.blur();
      }
    }

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [searchInputRef, onClear]);
}
