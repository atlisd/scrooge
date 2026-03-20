<script lang="ts">
  import pkg from '../../../../package.json';
  import { logout } from '$lib/api';
</script>

<div class="card shadow-sm">
  <div class="card-body">
    <h3 class="card-title mb-3">About Scrooge</h3>
    <p class="mb-1"><strong>Version:</strong> {pkg.version}</p>
    <p class="mb-3">
      <a href="https://github.com/atlisd/scrooge" target="_blank" rel="noopener noreferrer">
        View on GitHub
      </a>
    </p>
    <button class="btn btn-outline-danger btn-sm" onclick={logout}>Logout</button>
  </div>
</div>
